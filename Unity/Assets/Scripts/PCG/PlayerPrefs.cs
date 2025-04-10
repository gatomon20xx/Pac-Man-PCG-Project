using PCC.ContentRepresentation.Features;
using PCC.ContentRepresentation.Sample;
using PCC.CurationMethod;
using PCC.CurationMethod.Binary;
using PCC.Utility.Range;
using System;
using System.Collections.Generic;

public class PlayerPrefs
{
    private readonly List<Feature> m_features;

    private BRSCurator m_BRSCurator;

    public int Lessons { get; protected set; }
    
    public PlayerPrefs()
    {
        // Create the list of features
        m_features = new List<Feature>() {
            new Feature("pellet_density", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 1.0f) })),
            new Feature("power_pellets", new FloatRange(0.005f, 0.2f)),
            new Feature("small_stop", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("med_stop", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("large_stop", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("sizetwo_grow", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("sizefour_grow", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("topbot_mix", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.1f, 0.75f) })),
            new Feature("long_pieces", new IntRange(new List<Tuple<int, int>>() { new Tuple<int, int>(0, 4) })),
            new Feature("fruit_chance", new FloatRange(new List<Tuple<float, float>>() { new Tuple<float, float>(0.00f, 0.075f) }))
        };

        // Create the curator
        m_BRSCurator = new BRSCurator(m_features, 5, 10, true, 0.3f, 0.2f, 2);
    }

    public List<Sample> GenerateSample(int count, SampleGenerationMethod method)
    {
        return m_BRSCurator.GenerateSamples(count, method);
    }

    public Sample GenerateASample(SampleGenerationMethod method)
    {
        return m_BRSCurator.GenerateSamples(1, method)[0];
    }

    public void AssignPlayerPrefs(Sample sample, float i_newPlayerPref)
    {
        // Cast to an int
        m_BRSCurator.RecordSample(sample, (int)i_newPlayerPref);

        if (Lessons < 20)
            Lessons++;
    }
}
