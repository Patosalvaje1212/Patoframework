#version 330


in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0; 

uniform vec2 DownscaleRes;

void main()
{
    vec2 newFragTextCoord =  (round(fragTexCoord * DownscaleRes - DownscaleRes) / DownscaleRes) + (DownscaleRes / 4);
    
    newFragTextCoord = vec2(newFragTextCoord.r, -newFragTextCoord.g);

    vec4 DiffuseColor = texture(texture0, newFragTextCoord);


    gl_FragColor = DiffuseColor;

}