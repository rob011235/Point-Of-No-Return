//Shader by: perfoon
//Creative Commons: The shader code and all code snippets in this post are under CC0 license and can be used freely without the author's permission. Images and videos, and assets depicted in those, do not fall under this license. For more info, see our License terms.
//Source: https://godotshaders.com/shader/skybox-from-6-textures/

shader_type sky;

uniform sampler2D front : source_color;
uniform sampler2D back : source_color;
uniform sampler2D left : source_color;
uniform sampler2D right : source_color;
uniform sampler2D top : source_color;
uniform sampler2D bottom : source_color;

void sky() {
	COLOR = EYEDIR;
	vec3 absvec = abs(EYEDIR);
	float ma = max(max(absvec.x, absvec.y), absvec.z);
	vec3 dir = EYEDIR / (ma + 0.0001);

	if (EYEDIR.x >= ma) {
		COLOR = texture(left, dir.zy * vec2(-0.5, -0.5) - 0.5).rgb;
	}
	if (EYEDIR.x <= -ma) {
		COLOR = texture(right, dir.zy * vec2(0.5, -0.5) - 0.5).rgb;
	}
	if (EYEDIR.z >= ma) {
		COLOR = texture(front, dir.xy * vec2(0.5, -0.5) - 0.5).rgb;
	}
	if (EYEDIR.z <= -ma) {
		COLOR = texture(back, dir.xy * vec2(-0.5, -0.5) - 0.5).rgb;
	}
	if (EYEDIR.y >= ma) {
		COLOR = texture(top, dir.xz * vec2(0.5, 0.5) + 0.5).rgb;
	}
	if (EYEDIR.y <= -ma) {
		COLOR = texture(bottom, dir.xz * vec2(0.5, -0.5) + 0.5).rgb;
	}
}
