public static class MathUtils {
    public static float CalcularSimilaridadeCosseno(float[] vetorA, float[] vetorB) {
        if (vetorA.Length != vetorB.Length) return 0;

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < vetorA.Length; i++) {
            dotProduct += vetorA[i] * vetorB[i];
            magnitudeA += vetorA[i] * vetorA[i];
            magnitudeB += vetorB[i] * vetorB[i];
        }

        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0) return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}