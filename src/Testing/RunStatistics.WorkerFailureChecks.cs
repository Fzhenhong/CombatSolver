using System.Text.Json;

namespace CombatSolver;

internal sealed partial class RunStatistics
{
    // Exercise the actual Node producer/consumer boundary without enabling telemetry in a test run.
    internal static async Task AssertWorkerFailureIsolationAsync(string evidenceDirectory)
    {
        string directory = Path.Combine(evidenceDirectory, "corrupt-statistics");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "damaged.run.json");
        byte[] damaged = [0, 0, 0, 0];
        await File.WriteAllBytesAsync(path, damaged);
        using var instance = new RunStatistics();
        instance._run = new("test-run", "test-profile", 1, null, "IRONCLAD", 0, "test",
            "full", "pending", true, true, false, [], [], [], []);
        instance._snapshot = new(1, "test-profile", 1, new(1, 0, 0, 1, 1, 1, 1), null);
        instance._worker = Task.Run(() => instance.ProcessAsync(directory));
        try
        {
            await instance._worker;
            throw new InvalidOperationException("Corrupt statistics unexpectedly loaded.");
        }
        catch (JsonException exception) when (exception.BytePositionInLine == 0)
        {
            // The fixture must hit the historical store-constructor error, not a substitute fault.
        }
        instance._Process(0);
        for (int i = 0; i < 300; i++)
            instance.Enqueue(new("execute", instance._run, "battle-" + i, true, true, true));
        if (instance._run != null || instance._snapshot != null || instance._signals.Reader.Count != 0)
            throw new InvalidOperationException("Failed statistics retained a valid snapshot or queued gameplay events.");
        if (!File.ReadAllBytes(path).SequenceEqual(damaged))
            throw new InvalidOperationException("Statistics failure changed the corrupt source evidence.");
        Entry.Logger.Info("[CombatSolver/Unattended] RUN_STATISTICS_WORKER_FAILURE_OK original_json_fault 300_posts snapshot_invalidated evidence_preserved");
    }
}
