using UnityEngine;
using System.Collections;

public class TerrainNoise
{

    private static OpenSimplexNoise plexNoise = new OpenSimplexNoise();

    public static double eval(double x, double y, double z, double startFrequence, int octaves, double persistence)
    {
        double noise = 0;
        double normalizeFactor = 0;

        double frequence = startFrequence;

        double amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            normalizeFactor += amplitude;

            noise += amplitude * plexNoise.Evaluate(frequence * x, frequence * y, frequence * z);
            frequence *= 2;
            amplitude *= persistence;
        }

        return noise / normalizeFactor;
    }

    public static double getHeight(int x, int z)
    {
        return eval(x, z, 0, 0.004, 3, 0.75);
    }
}
