
#version 460

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inNormal;
layout (location = 2) in vec2 inUV;
//layout (location = 3) in vec4 inColor;


layout (location = 0) out vec3 outWorldPosition;
layout (location = 1) out vec3 outViewPosition;
layout (location = 2) out vec3 outNormal;
layout (location = 3) out vec2 outUV;
layout (location = 4) out vec4 outColor;

layout (set = 1, binding = 0) uniform Transform
{
	mat4 viewMatrix;
	mat4 projectionMatrix;
	mat4 modelMatrix;	
};

void main()
{
	// Transform world position
	vec4 worldPosition = modelMatrix * vec4(inPosition, 1.0);
		
	// Get world position
	outWorldPosition = worldPosition.xyz;
	
	// Get view position
	outViewPosition = vec3(0.0);

	// Get normal
	mat3 normalMat = transpose(inverse(mat3(modelMatrix)));
	outNormal = normalize(normalMat * inNormal);
	
	outUV = inUV;
	outColor = vec4(1.0);

	// Get position
    gl_Position = projectionMatrix * viewMatrix * worldPosition;
}