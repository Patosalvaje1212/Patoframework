#version 330


in vec2 fragTexCoord;
in vec4 fragColor;

//our texture samplers
uniform sampler2D texture0;   //diffuse map
uniform sampler2D texture1;   //normal map

//values used for shading algorithm...
uniform vec2 Resolution;     //resolution of screen
uniform vec4 LightColor[5];      //light RGBA -- alpha is intensity
uniform vec4 AmbientColor;    //ambient RGBA -- alpha is intensity 
uniform vec3 Falloff;         //attenuation coefficients

uniform vec3 LightPos[5];     //light position, normalized
uniform int LightCount;

uniform int lightResolution;

uniform vec2 cameraOffset;
uniform float cameraZoom;


void main()
{

	//RGBA of our diffuse color
	vec4 DiffuseColor = texture(texture0, fragTexCoord);
	
	//RGB of our normal map
	vec4 NormalMap = texture(texture1, fragTexCoord);
	
	vec3 N = normalize(NormalMap.rgb * 2.0 - 1.0);
	
	vec3 Intensity =  (AmbientColor.rgb / AmbientColor.a);
	

	for(int i = 0; i < LightCount; i++)
	{
		//The delta position of light
		vec3 LightDir = vec3((LightPos[i].rg) - (gl_FragCoord.rg / (Resolution.rg)), LightPos[i].b);
			
		//normalize our vectors
		vec3 L = normalize(LightDir);
		float D = length(LightDir);

		//Then perform "N dot L" to determine our diffuse term
		vec3 Diffuse = (LightColor[i].rgb * LightColor[i].a) * (max(dot(N, L), 0.0));

		float Attenuation = 1.0 / ( Falloff.x + (Falloff.y*D) + (Falloff.z*D*D) );


		Intensity += Diffuse * Attenuation;
	}


	//the calculation which brings it all together
	vec3 FinalColor = DiffuseColor.rgb * (round(Intensity * lightResolution) / lightResolution);
	
	gl_FragColor = (fragColor * vec4(FinalColor, DiffuseColor.a));

}