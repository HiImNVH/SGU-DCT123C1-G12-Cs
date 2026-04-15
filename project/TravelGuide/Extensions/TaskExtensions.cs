namespace TravelGuide.Extensions;

public static class TaskExtensions
{
    public static void ForgetAwait(this Task task)
    {
        // Intentionally fire-and-forget
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                Console.WriteLine($"[ForgetAwait] Error: {t.Exception}");
        });
    }
}