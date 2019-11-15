
namespace Terrain.Map {
    /// <summary>
    /// Layered map is a combination of one or more Height Maps and allows for 
    /// interacting with maps as a group.
    /// </summary>
    public class LayeredMap : HeightMap {

        /// <summary>
        /// Different layers in the height map.
        /// </summary>
        private HeightMap[] layers;
        /// <summary>
        /// The height map that can be edited.
        /// </summary>
        private int editable = 0;

        /// <summary>
        /// Constructs a height map with a given set of layers
        /// </summary>
        /// <param name="layers">Layers of the height map (different height maps)</param>
        public LayeredMap(params HeightMap[] layers) {
            this.layers = layers;
            this.editable = 0;
        }

        /// <summary>
        /// Constructs a Layered Height map with a given editable layer.
        /// </summary>
        /// <param name="editable">Editable layer.</param>
        /// <param name="layers">Various layers in the height map.</param>
        public LayeredMap(int editable, HeightMap[] layers) {
            this.layers = layers;
            this.editable = editable;
        }

        /// <summary>
        /// Adds height to the editable height map out of the set of height maps
        /// </summary>
        /// <param name="x">X position in Grid</param>
        /// <param name="y">Y position in Grid</param>
        /// <param name="change">Change in height to apply.</param>
        public void AddHeight(int x, int y, float change)
        {
            this.layers[this.editable].AddHeight(x, y, change);
        }

        /// <summary>
        /// Gets the height of the layered map at a position. This is
        /// the sum of all the various height map layers.
        /// </summary>
        /// <param name="x">X position in Grid</param>
        /// <param name="y">Y position in Grid</param>
        /// <returns>Sum of all height map heights at a given X and Y</returns>
        public float GetHeight(int x, int y)
        {
            float sum = 0;
            for (int i = 0; i < this.layers.Length; i++) {
                sum += this.layers[i].GetHeight(x, y);
            }
            return sum;
        }

        /// <summary>
        /// Checks if a point is in the bounds for all layers of this height map.
        /// </summary>
        /// <param name="x">X position in Grid</param>
        /// <param name="y">Y position in Grid</param>
        /// <returns>True if the point is in the bounds for ALL layers, false otherwise.</returns>
        public bool IsInBounds(int x, int y)
        {
            for (int i = 0; i < this.layers.Length; i++) {
                if (!this.layers[i].IsInBounds(x, y)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the height in the height map so the resultant sum of all layers will be 
        /// a given height. Will only change the editable height map.
        /// 
        /// Sets the height of the editable to be height - current (current is the sum of all
        /// height maps at the given x and y)
        /// </summary>
        /// <param name="x">X position in Grid</param>
        /// <param name="y">Y position in Grid</param>
        /// <param name="height">Target height to make layered map.</param>
        public void SetHeight(int x, int y, float height)
        {
            float current = GetHeight(x, y);
            this.layers[0].SetHeight(x, y, height - current);
        }
    }
}
