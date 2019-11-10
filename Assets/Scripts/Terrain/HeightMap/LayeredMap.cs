
public class LayeredMap : HeightMap {

    private HeightMap[] layers;

    public LayeredMap(params HeightMap[] layers) {
        this.layers = layers;
    }

    public void AddHeight(int x, int y, float change)
    {
        layers[0].AddHeight(x, y, change);
    }

    public float GetHeight(int x, int y)
    {
        float sum = 0;
        for (int i = 0; i < this.layers.Length; i++) {
            sum += layers[i].GetHeight(x, y);
        }
        return sum;
    }

    public bool IsInBounds(int x, int y)
    {
        for (int i = 0; i < this.layers.Length; i++) {
            if (!layers[i].IsInBounds(x, y)) {
                return false;
            }
        }
        return true;
    }

    public void SetHeight(int x, int y, float height)
    {
        float current = GetHeight(x, y);
        layers[0].SetHeight(x, y, height - current);
    }
}
