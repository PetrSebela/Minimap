# Conversion from spherical to cartesian coordinate system
- distortion will always occure on poles
- 1 real meter roughly equals 1 meter after projection
**Spherical mercator projection** - approximates earth as sphere and outputs coordinates on plane
**Eliptical mercator projection** - approximates earth as globe, more accurate but more computationaly expensive


# Polygon triangulation
**Polygon ears** - if polygon has vertex $x$ and diagonal between vertices $x_{i-1}$ and $x_{i+1}$ lies entirely insede of said polygon, than vertex $x$ is a ear
**Ear clipping algorithm** - find ear of polygon, construct triangle and clip its ear. Repeat until all ears are clipped 


# Data structure
Map data is converted into generic objects based on tag types (Nodes, Building, etc...) which are stored in dictionary based on their id. 
Converted objects which are going to be renderer are then organised into chunks which are then organised into dictionary and assigned their global position as key.

## Chunk
- position
- list of nodes (single point data structures such as trees and lamps)
- list of buildings


*Depending on future of this project, I might need to switch to something like quad-tree data structure to handle displaying larger amounts of data at once*

# Cashing
1. Download said area (chunk)
2. Convert


# Open Street Map xml
## Nodes
## Ways
### Buildings
- perimeter is enclosed by default, no need to create closed perimeter
## Relations