using System.Text.Json;

namespace LevelUp;

// represents one exercise entry as it gets saved to disk
public class SavedExercise
{
    public string Name   { get; set; } = "";
    public double Weight { get; set; }
    public int    Reps   { get; set; }
    public bool   Done   { get; set; }

    public override string ToString() => $"{Name} \u2014 {Weight}kg x {Reps}";
}

// one workout session = a date + the exercises logged that day
public class WorkoutSession
{
    public string             Date      { get; set; } = "";
    public List<SavedExercise> Exercises { get; set; } = new();
}
