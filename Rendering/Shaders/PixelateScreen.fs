#version 330


in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0; 

uniform vec2 DownscaleRes = vec2(190, 100);

void main()
{
    vec2 newFragTextCoord = - round(fragTexCoord * DownscaleRes - DownscaleRes) / DownscaleRes;

    vec4 DiffuseColor = texture(texture0, newFragTextCoord);


    gl_FragColor = DiffuseColor;

}