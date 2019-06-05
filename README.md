# AnimTexPrj
unity gpu animation

Overview
AnimTexture convert bone’animation to vertex’s animation, use MeshRenderer rendering instead of SkinnedMeshRenderer. So support gpu instancing for lots characters.
Support any number of bones,any precision of animtionClips.
Size of Anim’Texture limited by:
	Horizontal: mesh ‘vertex.
	Vertical : AnimationClip’s time length
So more less vertex and more short time smaller animTexture.
Need more precision rendering. You can use Tesselation shader.

Basic work flow is
1.	bake animation’s mesh to an atlas(include all animationClip),
2.	renderer animation use animTexture.
3.	embed into bone.
Version 
0.1

Unity version
	Developed in unity 2018.3.0.
	Scripting runtime version : .NET 4.X.
	below this, need change something maybe.

Features
1.	Bake animationClips to a texture
2.	Support Mecanim worlflow by Play AnimTexture with Animator
3.	Support Blend 2 Animation
4.	Support gpu instancing for lots of animation Character.
	
SkinnedAnimation vs AnimTexture:
Name	SkinnedMesh	AnimTexture
Need skinned calculate	Y	N
Bone count limit	Y	N
Support Embed into bone	Y	y
Need bone data	Y	N
Need AnimationClip	Y	Y,when Embed into bone
AnimTexture More Memory	N	Y,format:RGBHalf
