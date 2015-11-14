using UnityEngine;
using System.Collections;

public static class NetworkDebugSettings
{
    public static bool LocalPlayerPrediction = true;  // Turns RewindReplay log functionality on and off for the local player
    public static bool RemotePlayerExtrapolation = false; // Turns extrapolation for remote players on and off
    public static bool ObjectInterpolation = true; // Turns object interpolation on/off
    public static bool PlayInputLocally = true; // Turns local input effects on/off
    public static bool ShowDebugIndicators = false; // Turns the RewindReplay and server position debug indicators
    public static float SimulatedPacketDropPct = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection
    public static int SimulatedLatency = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection
}
