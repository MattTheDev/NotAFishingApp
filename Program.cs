using System.Runtime.InteropServices;
using NAudio.Wave;

namespace ConsoleApp2;

class Program
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan,
        uint dwFlags, UIntPtr dwExtraInfo);

    private const byte VkR = 0x52; // Virtual Key Code for 'R'
    private const uint KeyeventfKeydown = 0x0000; // Key down event
    private const uint KeyeventfKeyup = 0x0002;   // Key up event

    static bool _amFishing;

    static async Task Main()
    {
        await Task.Delay(5000);
        
        PoleInteraction();
        _amFishing = true;

        using var waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 1;
        waveIn.WaveFormat = new WaveFormat(44100, 16, 1); // 44.1kHz, 16-bit, mono
        waveIn.DataAvailable += OnDataAvailable;
        waveIn.StartRecording();

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    // Interact with fishing pole
    static void PoleInteraction()
    {
        var random = new Random();

        // Press key
        keybd_event(VkR, 0, KeyeventfKeydown, UIntPtr.Zero);
        Thread.Sleep(random.Next(750, 1500)); // Wait for 2 seconds

        // Release key
        keybd_event(VkR, 0, KeyeventfKeyup, UIntPtr.Zero);
        Thread.Sleep(random.Next(500, 1250)); // Wait for 1 second
    }

    // Audio listener
    static void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        // e.Buffer contains the audio data
        var maxVolume = CalculateVolume(e.Buffer, e.BytesRecorded);

        // If audio breaches allowed limit ...
        if (maxVolume > 0.1)
        {
            // If not fishing, return.
            if (!_amFishing)
            {
                return;
            }

            // Trigger fishing interaction
            TriggerInteraction();
        }
    }

    // Calculate audio volume
    static float CalculateVolume(byte[] buffer, int bytesRecorded)
    {
        // Convert byte buffer to short samples
        var maxVolume = 0f;
        for (int i = 0; i < bytesRecorded; i += 2)
        {
            var sample = BitConverter.ToInt16(buffer, i);
            var volume = Math.Abs(sample / 32768f); // Normalize to range [0, 1]
            if (volume > maxVolume)
            {
                maxVolume = volume;
            }
        }

        return maxVolume;
    }

    // Reel in fish, wait, and cast pole again.
    static void TriggerInteraction()
    {
        var random = new Random();
        Thread.Sleep(random.Next(500,1500));

        PoleInteraction();
        _amFishing = false;

        Thread.Sleep(random.Next(2500,5000));
        PoleInteraction();
        _amFishing = true;
    }
}
