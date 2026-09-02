#version 460
precision highp float;

layout (location = 0) in vec3 inWorldPosition;
layout (location = 1) in vec3 inViewPosition;
layout (location = 2) in vec3 inNormal;
layout (location = 3) in vec2 inUV;
layout (location = 4) in vec4 inColor;

layout (location = 0) out vec4 outColor;

// material textures
layout(set = 2, binding = 0) uniform sampler2D albedoMap;
layout(set = 2, binding = 1) uniform sampler2D normalMap;
layout(set = 2, binding = 2) uniform sampler2D metallicMap;
layout(set = 2, binding = 3) uniform sampler2D roughnessMap;
layout(set = 2, binding = 4) uniform sampler2D aoMap;

// single directional light
vec3 lightDirection = vec3(0.0, -1.0, 0.0); // should point from light → surface
vec3 lightColor     = vec3(1.0, 1.0, 1.0);
float lightIntensity = 1.0;

const float PI = 3.14159265359;

vec3 getNormal(vec3 worldPos, vec3 normal, vec2 uv, sampler2D normalMap)
{
    // Sample the normal map
    vec3 mapN = texture(normalMap, uv).rgb;
	
	if (length(mapN - vec3(1.0)) < 0.01)
		return normal;

    // Treat textures that are close to neutral as "no normal map"
    if (length(mapN - vec3(0.5, 0.5, 1.0)) < 0.01) {
        return normalize(normal);
    }

    // Remap from [0,1] -> [-1,1]
    vec3 tangentNormal = mapN * 2.0 - 1.0;

    // Compute world-space TBN using derivatives (robust if no tangent attributes)
    vec3 Q1 = dFdx(worldPos);
    vec3 Q2 = dFdy(worldPos);
    vec2 st1 = dFdx(uv);
    vec2 st2 = dFdy(uv);

    // Compute tangent and bitangent
    float det = st1.x * st2.y - st2.x * st1.y;
    vec3 T = normalize((Q1 * st2.y - Q2 * st1.y) / max(det, 1e-6));
    vec3 B = normalize(cross(normal, T));

    mat3 TBN = mat3(T, B, normal);

    // Transform tangent-space normal to world space
    return normalize(TBN * tangentNormal);
}

// ----------------------------------------------------------------------------
vec3 getNormalFromMap(vec3 worldPos, vec3 normal)
{
    vec3 tangentNormal = texture(normalMap, inUV).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(worldPos);
    vec3 Q2  = dFdy(worldPos);
    vec2 st1 = dFdx(inUV);
    vec2 st2 = dFdy(inUV);

    vec3 N = normalize(inNormal);
    vec3 T = normalize((Q1 * st2.y - Q2 * st1.y) / (st1.x * st2.y - st2.x * st1.y));
    vec3 B = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
// ----------------------------------------------------------------------------
void main()
{
    // material parameters from textures
    vec3 albedo     = pow(texture(albedoMap, inUV).rgb, vec3(2.2));
    float metallic  = texture(metallicMap, inUV).r;
    float roughness = texture(roughnessMap, inUV).r;
    float ao        = texture(aoMap, inUV).r;

    // get normal from map
    vec3 N = getNormal(inWorldPosition, inNormal, inUV, normalMap); //inNormal;// getNormalFromMap(inWorldPosition, inNormal);

    // view vector (camera direction)
    vec3 V = normalize(inViewPosition - inWorldPosition);
    vec3 L = normalize(-lightDirection); // light direction points toward surface
    vec3 H = normalize(V + L);

    // base reflectance
    vec3 F0 = mix(vec3(0.04), albedo, metallic);

    // Cook–Torrance BRDF
    float NDF = DistributionGGX(N, H, roughness);
    float G   = GeometrySmith(N, V, L, roughness);
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular = numerator / denominator;

    // energy conservation
    vec3 kS = F;
    vec3 kD = (1.0 - kS) * (1.0 - metallic);

    float NdotL = max(dot(N, L), 0.0);
    vec3 radiance = lightColor * lightIntensity; // no attenuation for directional light

    // direct lighting
    vec3 Lo = (kD * albedo / PI + specular) * radiance * NdotL;

    // ambient (AO)
    vec3 ambient = vec3(0.03) * albedo * ao;

    vec3 color = ambient + Lo;

    // HDR tonemapping + gamma correction
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0 / 2.2));

    outColor = vec4(color, 1.0);
	//outColor = vec4(normalize(inNormal)*0.5+0.5, 1.0);
}
