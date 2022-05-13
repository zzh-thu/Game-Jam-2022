# Game-Jam-2022

参与人员：张若恬，沈心云，徐妙，郑至恒，王曙源

## 项目文件夹目录架构

* ../
  * **AssetsPackages**  *所有的游戏资源，3d图形，图片，视频，音频等*
    * Animations
    * Audio
      * Music
      * SFX
    * Materials
    * Models
    * Plugins
    * Prefabs
    * Textures
    * GUI
    * Shaders
  * **~~Edito~~r**  *---可能用不到---* *框架与项目编辑器扩展代码*
  * **Scenes**  *所有的游戏场景，包括运行场景,地图编辑场景,角色编辑场景,特效编辑场景*
    * Level
    * Orther
  * **Scripts**  *所有游戏的代码，含框架，游戏组件与游戏逻辑代码。*
    * Component
    * Logic
    * Orher
  * **~~3rd~~** *---可能用不到---* *第三方SDK和插件*
  * **~~SteammingAssets~~** *---可能用不到---*  *与Ab资源包管理更新相关的代码。*

## 场景层次结构

以下表示为某Scenes中用于组织结构的空物体的结构。

* Some-Scenes
  
  * Management *该场景的具体的逻辑脚本，包括关于加载预制体，初始化数据，缓存池等等*
  * GUI *UI组件*
  * Cameras *摄像机组件*
  * Lights *灯光*
  * World *地图*
  * _Dynamic *所有的预制体实例*
  
  命名注意事项：

1. * 所有的空物体应保持在`Vector3.zero`，默认旋转角度和缩放比例。
2. 当您在运行时实例化(克隆) 对象时，请确保将其放在 _Dynamic 中 – **不要污染层次结构的根目录**，否则您将发现难以浏览它。
3.  对于仅作为脚本容器的空对象，请使用“@”作为前缀 – 例如@Cheats。
