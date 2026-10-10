// Owned process fixture: never dispatches arbitrary shell commands.
Console.OutputEncoding = new System.Text.UTF8Encoding(false);
switch (args[0])
{
    case "exact":
        Console.Write(args[1]);
        await Console.Error.WriteAsync("private diagnostic must not escape");
        break;
    case "delay":
        await File.WriteAllTextAsync(args[1], Environment.ProcessId.ToString());
        await Task.Delay(TimeSpan.FromMinutes(1));
        break;
    case "invalid-utf8":
        await File.WriteAllTextAsync(args[1], Environment.ProcessId.ToString());
        using (var output = Console.OpenStandardOutput())
        {
            await output.WriteAsync(new byte[] { 0xff, 0xff });
            await output.FlushAsync();
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
        break;
    default:
        return 2;
}
return 0;
