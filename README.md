# NetRunner
NetRunner is a simple neural network visualizer created in Unity3D. 

![Alt Text](netrunner.gif)


At this point it's a proof of concept highlighting the use of Unity's inbuilt Baracuda library to load and run neural network models converted from ONNX.
This example demonstrates a simple two layer dense network trained in Keras on MNIST dataset and 
converted to ONNX via Keras2ONNX library [ https://pypi.org/project/keras2onnx/ ]

The rendering elements are made using Unity's HDRP pipeline and free assets.


# Neural net explainer
<a href="http://www.youtube.com/watch?feature=player_embedded&v=qtGEB-TtEP4
" target="_blank"><img src="http://img.youtube.com/vi/qtGEB-TtEP4/0.jpg" 
alt="Neural net primer" width="240" height="180" border="10" /></a>

I made this youtube video explaining some basic NN concepts by utilizing this demo.

# How to try
* Install Unity 2019.4.18f1 or later
* Clone this repo or download zip
* Import project via Unity Hub
* Build and enjoy!

# TODO
- [x] Dense net implementation
- [ ] Conv net implementation
- [ ] Generic layers
- [ ] Refactoring
- [ ] Ability to load any ONNX model 

# Issues
If you run into any issues or want a feature, please open an issue.
