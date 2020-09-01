Perlin Noise Image Generator for Unity
===============================

This prject include scenes that show case 1D/2D/3D perline noise.

And a scene that visualize 3D noise with a flow field.

And then, there's a scene that can preview/export noise image in unity editor.

![noiseGenerator](https://user-images.githubusercontent.com/13420668/71320856-6397dc80-24ec-11ea-8b59-93f3919229f5.gif)


(Edit) 3D noise with a flow field(Implement C# Job System to it)
===============================
40 * 40 * 40 pelin noise flow field with octaves level set by 3

Without using job system(single thread, 10fps)
---
<img width="696" alt="Screen Shot 2020-06-05 at 9 49 49 PM" src="https://user-images.githubusercontent.com/13420668/83883774-85bda800-a776-11ea-9c7c-775dde3e8f83.png">

With job system(burst compiled, multithread, 90+fps)
---
<img width="684" alt="Screen Shot 2020-06-05 at 9 49 42 PM" src="https://user-images.githubusercontent.com/13420668/83883755-80f8f400-a776-11ea-801a-99bd721407ec.png">

Creadits
===============================

I use the perlin noise library from [Kejiro's github] to Generate perlin noise texture.

[Kejiro's github]: https://github.com/keijiro/PerlinNoise
