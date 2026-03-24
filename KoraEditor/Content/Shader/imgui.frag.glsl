#version 450

layout(location = 0) in vec2 frag_uv;
layout(location = 1) in vec4 frag_color;

layout(location = 0) out vec4 out_color;

layout(set = 2, binding = 0) uniform sampler2D tex;

void main()
{
    out_color = frag_color * texture(tex, frag_uv);
	//out_color = vec4(1,0,0,1);
	
	
	
	vec4 s = texture(tex, frag_uv);

// glyph mask: prefer alpha, else use rgb luminance (covers RGBA vs R8 atlas mismatches)
float mask = s.a;
if (mask <= 0.0001)
    mask = dot(s.rgb, vec3(0.333333));

// swizzle vertex color if needed (handles packed UBYTE4 ordering); change to plain frag_color if you know it's RGBA
vec4 vc = frag_color.bgra;

// combine: multiply texture mask with vertex color, non-premultiplied alpha
out_color.rgb = vc.rgb * mask;
out_color.a = vc.a * mask;
}