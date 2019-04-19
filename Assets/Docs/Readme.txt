==========================
==		动画纹理
==========================
概述:
	1 将骨骼动画转化为顶点动画,结合gpu instancing完成大规模动画角色的渲染.
	2 支持任意数量的骨骼,任何精度级别的动画.
	3 动画纹理的大小受限于(顶点数量,动画的时间)
		水平像素:顶点的数量
		竖直像素:动画的逐帧数据
		顶点少,动画片段少,动画时间短,可以减少生成的动画纹理的大小.
	4 网格渲染的细节,可以通过细分shader来增加

优劣势对比:
	名称						蒙皮(cpu,gpu)			动画纹理
	需要蒙皮计算(cpu,gpu)		有						无
	骨骼数量限制				有						无
	挂点骨骼					需要						需要
	需要骨骼数据				需要						挂点功能需要
	动画剪辑内存				需要						挂点功能需要
	动画纹理					无						有,纹理格式:rgbHalf

----------------------------------------------
unity蒙皮动画的工作流程:
	cpu,simd
		cpu动画驱动骨骼 -> cpu骨骼驱动顶点 -> 变换后的顶点发给gpu渲染
	gpu skin:
		cpu动画驱动骨骼 ->gpu上蒙皮 ->写回主存 -> 变换后的顶点发给gpu渲染

动画纹理的原理
	1 逐帧将动画驱动的mesh(顶点数据)烘焙到纹理,使用MeshRenderer结合gpu instancing来渲染.
		渲染时无蒙皮的计算.
		不需要挂点时,无cpu的骨骼变换
	2 在顶点shader中获取对应的数据来控制顶点以复原动画的播放.
	3 动画纹理的构成
		动画1
			水平: 动画驱动的顶点数据(xyz)
			竖直: 动画帧
		动画2
			水平: 动画驱动的顶点数据(xyz)
			竖直: 动画帧

注意事项.
	1 材质需要开启gpu instancing.
	2 TextureAnimation 组件的 manifest(运行时会赋值动画纹理)
	3 MeshRenderer/Anim Tex,动画纹理须存在.

使用.
	1 bake mesh
		* bake时,模型AnimationType需要为Legacy.
			播放时,不需要.
		1 有动画的模型拖放到Hierarchy.
			或 Hierarchy建EmptyObject,起名AnimPlayer,附加MeshRenderer,MeshFllter,附加Animation及animtionClips,拖到Assets中形成prefab.
		2 选择该prefab,点击AnimTexture/BakeAnimToAtlas
			AnimTexPath会出现bake的动画纹理与动画纹理信息文件.
	2 播放动画
		1 prefab拖到Hierarchy
			1 附加MeshFilter,选模型mesh
			2 附加MeshRenderer.
			3 TextureAnimation组件.
		2 建材质
			1 shader选AnimTexture.
			2 拖到MeshRenderer的material槽.
		3 将动画纹理信息文件拖给TextureAnimation的TexInfo槽位.
		3 可直接使用TextureAnimation控制动画播放.
	4 使用Animator
		* 将模型(及动画模型)AnimationType置为Generic.
		* 无挂点,不需要使用动画剪辑.
		1 附加Animator到prefab,并做好AnimatorController
		2 附加MecanimController组件
			该组件会获取Animator的状态与过度,调用TextureAnimation播放动画
		3 控制Animator的参数进行动画.
	5 使用NavMeshAgent
		1 Hierarchy中建Empty,起名AgentPlayer.
			附加NavMeshAgent.
		2 将AnimPlayer拖到AgentPlayer下
		3 控制agent.destination.
	6 使用骨骼作为装备的挂点
		* 将模型(及动画模型)AnimationType置为Generic.
		1 开启Optimize Game Objects,并保留期望的骨骼.
		2 Animator Controller中动画状态放入对应的动画剪辑


==========================
==		Classes
==========================
Animation
	MecanimController //通过Animator状态机控制TextureAnimation的播放
	TextrueAnimation // 控制shader的参数来控制动画的播放.
Info
	AnimTextureManifest //动画纹理清单
Tools
	AgentPlayer // NavMeshAgent寻路
	Fps //现实 fps
	Spawner // 批量创建 prefab
	TestAnimTexture //测试BakeMesh的渲染.
Utils
	AnimToTextureUtils //播放Animation,bakeMesh
Shaders
	AnimTexture.shader //通过纹理动画及参数,控制动画播放.
Editor
	AnimTextureEditor //将选择的有动画的模型,烘焙动画进纹理.
	AnimTexturePlayerCreator //创建一个可以运行的智能体.
		创建的player大致的结构:
		NavMeshPlayer
			Animator