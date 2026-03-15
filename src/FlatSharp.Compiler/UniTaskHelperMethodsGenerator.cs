namespace FlatSharp.Compiler;

internal static class UniTaskHelperMethodsGenerator
{
    private const string CysharpCore = "Cysharp.Threading.Tasks";
    private const string GrpcCore = "Grpc.Core";

    private const string UniTask = CysharpCore + ".UniTask";
    private const string IUniTaskAsyncEnumerable = CysharpCore + ".IUniTaskAsyncEnumerable";
    private const string IUniTaskAsyncEnumerator = CysharpCore + ".IUniTaskAsyncEnumerator";
    private const string CancellationToken = "System.Threading.CancellationToken";

    public static void GenerateUniTaskHelpers(CodeWriter writer)
    {
        writer.AppendLine("public static class UniTaskHelpers");
        using (writer.WithBlock())
        {
            AppendAsServerMethod(writer);

            writer.AppendLine(
                $"public static {IUniTaskAsyncEnumerable}<T> AsUniTaskAsyncEnumerable<T>(this {GrpcCore}.IAsyncStreamReader<T> reader)");
            using (writer.WithBlock())
            {
                writer.AppendLine("return new AsyncStreamReaderAsUniTaskAsyncEnumerableWrapper<T>(reader);");
            }

            writer.AppendLine(
                $"public static IUniTaskAsyncStreamWriter<T> AsUniTaskAsyncStreamWriter<T>(this {GrpcCore}.IAsyncStreamWriter<T> writer)");
            using (writer.WithBlock())
            {
                writer.AppendLine("return new UniTaskAsyncStreamWriterWrapper<T>(writer);");
            }
        }

        AppendAsyncStreamReaderWrapper(writer);
        AppendAsyncStreamWriterWrapper(writer);
        AppendIUniTaskAsyncStreamWriter(writer);
    }

    private static void AppendAsServerMethod(CodeWriter writer)
    {
        writer.AppendLine(
            $"public static {GrpcCore}.UnaryServerMethod<TRequest, TResponse> AsServerMethod<TRequest, TResponse>(Func<TRequest, {GrpcCore}.ServerCallContext, {UniTask}<TResponse>> asyncDelegate)");
        AppendTypeConstraints(writer);

        using (writer.WithBlock())
        {
            writer.AppendLine("return (request, response) => asyncDelegate(request, response).AsTask();");
        }

        writer.AppendLine(
            $"public static {GrpcCore}.ClientStreamingServerMethod<TRequest, TResponse> AsServerMethod<TRequest, TResponse>(Func<IUniTaskAsyncEnumerable<TRequest>, {GrpcCore}.ServerCallContext, {UniTask}<TResponse>> asyncDelegate)");
        AppendTypeConstraints(writer);

        using (writer.WithBlock())
        {
            writer.AppendLine(
                "return (request, context) => asyncDelegate(request.AsUniTaskAsyncEnumerable(), context).AsTask();");
        }

        writer.AppendLine(
            $"public static {GrpcCore}.ServerStreamingServerMethod<TRequest, TResponse> AsServerMethod<TRequest, TResponse>(Func<TRequest, IUniTaskAsyncStreamWriter<TResponse>, {GrpcCore}.ServerCallContext, {UniTask}> asyncDelegate)");
        AppendTypeConstraints(writer);

        using (writer.WithBlock())
        {
            writer.AppendLine(
                "return (request, response, context) => asyncDelegate(request, response.AsUniTaskAsyncStreamWriter(), context).AsTask();");
        }

        writer.AppendLine(
            $"public static {GrpcCore}.DuplexStreamingServerMethod<TRequest, TResponse> AsServerMethod<TRequest, TResponse>(Func<{IUniTaskAsyncEnumerable}<TRequest>, IUniTaskAsyncStreamWriter<TResponse>, {GrpcCore}.ServerCallContext, {UniTask}> asyncDelegate)");
        AppendTypeConstraints(writer);

        using (writer.WithBlock())
        {
            writer.AppendLine(
                "return (request, response, context) => asyncDelegate(request.AsUniTaskAsyncEnumerable(), response.AsUniTaskAsyncStreamWriter(), context).AsTask();");
        }
    }

    private static void AppendTypeConstraints(CodeWriter writer)
    {
        writer.AppendLine("where TRequest : class");
        writer.AppendLine("where TResponse : class");
    }

    private static void AppendAsyncStreamReaderWrapper(CodeWriter writer)
    {
        writer.AppendLine($"public sealed class AsyncStreamReaderAsUniTaskAsyncEnumerableWrapper<T> : {IUniTaskAsyncEnumerable}<T>");
        using (writer.WithBlock())
        {
            writer.AppendLine();
            writer.AppendLine($"private readonly {GrpcCore}.IAsyncStreamReader<T> _reader;");
            writer.AppendLine();
            writer.AppendLine($"public AsyncStreamReaderAsUniTaskAsyncEnumerableWrapper({GrpcCore}.IAsyncStreamReader<T> reader)");

            using (writer.WithBlock())
            {
                writer.AppendLine("_reader = reader;");
            }

            writer.AppendLine($"public {IUniTaskAsyncEnumerator}<T> GetAsyncEnumerator({CancellationToken} cancellationToken = default)");
            using (writer.WithBlock())
            {
                writer.AppendLine("return new Enumerator(_reader, cancellationToken);");
            }

            writer.AppendLine($"public sealed class Enumerator : {IUniTaskAsyncEnumerator}<T>");
            using (writer.WithBlock())
            {
                writer.AppendLine($"private readonly {GrpcCore}.IAsyncStreamReader<T> _reader;");
                writer.AppendLine($"private readonly {CancellationToken} _cancellationToken;");
                writer.AppendLine();
                writer.AppendLine(
                    $"public Enumerator({GrpcCore}.IAsyncStreamReader<T> reader, {CancellationToken} cancellationToken)");
                using (writer.WithBlock())
                {
                    writer.AppendLine("_reader = reader;");
                    writer.AppendLine("_cancellationToken = cancellationToken;");
                }

                writer.AppendLine("public T Current => _reader.Current;");
                writer.AppendLine(
                    $"public {UniTask}<bool> MoveNextAsync() => _reader.MoveNext(_cancellationToken).AsUniTask();");
                writer.AppendLine($"public {UniTask} DisposeAsync() => {UniTask}.CompletedTask;");
            }
        }
    }

    private static void AppendAsyncStreamWriterWrapper(CodeWriter writer)
    {
        writer.AppendLine("public sealed class UniTaskAsyncStreamWriterWrapper<T> : IUniTaskAsyncStreamWriter<T>");
        using (writer.WithBlock())
        {
            writer.AppendLine($"private readonly {GrpcCore}.IAsyncStreamWriter<T> _writer;");

            writer.AppendLine($"public UniTaskAsyncStreamWriterWrapper({GrpcCore}.IAsyncStreamWriter<T> writer)");
            using (writer.WithBlock())
            {
                writer.AppendLine("_writer = writer;");
            }

            writer.AppendLine();

            writer.AppendLine(
                $"public {GrpcCore}.WriteOptions? WriteOptions {{ get => _writer.WriteOptions; set => _writer.WriteOptions = value; }}");

            writer.AppendLine(
                $"public {UniTask} WriteAsync(T message, {CancellationToken} cancellationToken) => _writer.WriteAsync(message, cancellationToken).AsUniTask();");
        }
    }

    private static void AppendIUniTaskAsyncStreamWriter(CodeWriter writer)
    {
        writer.AppendLine("public interface IUniTaskAsyncStreamWriter<in T>");
        using (writer.WithBlock())
        {
            writer.AppendSummaryComment("Writes a message asynchronously. Only one write can be pending at a time.");
            writer.AppendLine($"{UniTask} WriteAsync(T message, {CancellationToken} cancellationToken);");

            writer.AppendSummaryComment(
                "Write options that will be used for the next write.",
                "If null, default options will be used.",
                "Once set, this property maintains its value across subsequent",
                "writes");

            writer.AppendLine($"{GrpcCore}.WriteOptions? WriteOptions {{ get; set; }}");
        }
    }
}