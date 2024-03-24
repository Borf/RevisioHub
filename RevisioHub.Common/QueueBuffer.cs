namespace RevisioHub.Common;

public class QueueBuffer
{
    private byte[] buffer = new byte[1024];
    private int start = 0;
    private int end = 0;

    public int Length { get => end - start; }

    public void addData(Span<byte> data)
    {
        //resize buffer if not large enough
        if (buffer.Length < end + data.Length)
        {
            int newLength = buffer.Length * 2;
            while (newLength < data.Length + (end - start))
                newLength *= 2;

            byte[] newBuffer = new byte[newLength];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, end);
            buffer = newBuffer;
            end -= start;
            start = 0;
        }
        data.CopyTo(new Span<byte>(buffer, end, data.Length));
        end += data.Length;
    }

    public ReadOnlySpan<byte> GetBytes(int byteCount)
    {
        if (byteCount > end - start)
            byteCount = end - start;
        var span = new ReadOnlySpan<byte>(buffer, start, byteCount);
        start += byteCount;
        if (start == end) // read the full buffer, so we can start new
        {
            start = 0;
            end = 0;
        }
        else if (start > 1024 * 1024)
        {
            byte[] newBuffer = new byte[end - start]; // no growth? is that good?
            Buffer.BlockCopy(buffer, start, newBuffer, 0, end - start);
            end -= start;
            start = 0;
        }
        return span;
    }

    public ReadOnlySpan<byte> PeekBytes(int byteCount)
    {
        if (byteCount > end - start)
            byteCount = end - start;
        return new ReadOnlySpan<byte>(buffer, start, byteCount);
    }


}
