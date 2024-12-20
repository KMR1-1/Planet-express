# simple case
- if mesh has Texture
  - process texture

# not to miss case
- if node has texture
  - create vnodes for meshes
  - process each meshes
     - apply texture to meshes

# other case
- if node has aplha prop
  - apply aplha to all meshes texture

# in the process of struct node
- Get NiNode
- create Mesh instance in Struct.cs
  - setting in Mesh.cs
  - create Material instance    
    - setting in Material.cs
  - adding Material to gltf in Mesh.cs
- adding mesh to gltf in Struct.cs



NiNode :
NiStencilProperty
NiVertexColorProperty
NiAlphaProperty
NiZBufferProperty
NiTexturingProperty

NiTriStrips : 
NiVertexColorProperty
NiTexturingProperty
NiMaterialProperty
NiAlphaProperty
NiZBufferProperty
