// Owned process fixture: never dispatches arbitrary shell commands.
Console.OutputEncoding = new System.Text.UTF8Encoding(false);
switch (args[0])
{
    case "exact":
        Console.Write(args[1]);
        Console.Error.Write("private diagnostic must not escape");
        break;
    case "delay":
        File.WriteAllText(args[1], Environment.ProcessId.ToString());
        await Task.Delay(TimeSpan.FromMinutes(1));
        break;
    case "invalid-utf8":
        File.WriteAllText(args[1], Environment.ProcessId.ToString());
        using (var output = Console.OpenStandardOutput())
        {
            output.Write([0xff, 0xff]);
            output.Flush();
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
        break;
    default:
        return 2;
}
return 0;
