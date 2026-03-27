#version 450

layout(location = 0) in vec2 frag_uv;
layout(location = 1) in vec4 frag_color;

layout(location = 0) out vec4 out_color;

layout(set = 2, binding = 0) uniform sampler2D tex;

void main()
{
    out_color = frag_color * texture(tex, frag_uv);
	//out_color = vec4(1,0,0,1);
	
}