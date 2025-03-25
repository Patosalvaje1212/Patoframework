/*version 430

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform sampler2D texture1;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add your custom variables here

uniform sampler2D normalMap;

struct Light
{
    int enabled;
    vec2 position;
};

uniform vec2 position1 = vec2(15.0, 15);
uniform vec2 position2;


void main()
{
    // Texel color fetching from texture sampler
    vec4 texelColor = texture(texture0, fragTexCoord) * colDiffuse * fragColor;

    vec3 textelMap = texture(texture1, fragTexCoord).rgb;

    textelMap = (textelMap * 2) + 1;

    vec2 lightDir = normalize(position1.rg - gl_FragCoord.rg);


    float dotPos = max(dot(textelMap.rg, lightDir), 0.0);


    // Convert texel color to grayscale using NTSC conversion weights
    

    // Calculate final fragment color
    finalColor = vec4(texelColor * vec4(vec3(dotPos), 0.0));
    finalColor += vec4(0.4)/1.0;


    //finalColor = pow(finalColor, vec4(1.0/2.2));
}*/


//attributes from vertex shader
//attributes from vertex shader
#version 330


in vec2 fragTexCoord;
in vec4 fragColor;

//our texture samplers
uniform sampler2D texture0;   //diffuse map
uniform sampler2D texture1;   //normal map

//values used for shading algorithm...
uniform vec2 Resolution;     //resolution of screen
uniform vec4 LightColor = vec4(1.0, 1.0, 1.0, 1.0);      //light RGBA -- alpha is intensity
uniform vec4 AmbientColor = vec4(0.5, 0.5, 0.5, 0.8);    //ambient RGBA -- alpha is intensity 
uniform vec3 Falloff = vec3(0.8);         //attenuation coefficients

uniform int lightResolution = 150;
uniform vec3 LightPos[5];     //light position, normalized
uniform float Rotation = 0.0;

uniform vec2 cameraOffset;
uniform float cameraZoom;



void main()
{
	//RGBA of our diffuse color
	vec4 DiffuseColor = texture(texture0, fragTexCoord);
	
	//RGB of our normal map
	vec4 NormalMap = texture(texture1, fragTexCoord);
	
	vec3 N = normalize(NormalMap.rgb * 2.0 - 1.0);
	
	vec3 Intensity = vec3(0.0);

	for(int i = 0; i < 5; i++)
	{
		//The delta position of light
		vec3 LightDir = vec3((LightPos[i].xy) - (gl_FragCoord.xy / (Resolution.xy)), LightPos[i].z);
			
		//normalize our vectors
		vec3 L = normalize(LightDir);

		//Then perform "N dot L" to determine our diffuse term
		vec3 Diffuse = vec3(1.0) * max(dot(N, L), 0.0);

		Intensity += Diffuse + (AmbientColor.rgb * AmbientColor.a);
	}


	Intensity /= 5;
	

	//the calculation which brings it all together
	
	
	vec3 FinalColor = min((DiffuseColor.rgb * (round(Intensity * lightResolution)) / lightResolution),  DiffuseColor.rgb);
	
	gl_FragColor = fragColor * vec4(FinalColor, DiffuseColor.a);

}

vec2 rotate(vec2 v, float a)
{
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(c, s, -s, c);
	return m * v;
}

