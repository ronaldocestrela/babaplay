using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SoccerPosition
{
    GK,     // Goalkeeper

    RB,     // Right Back
    LB,     // Left Back
    CB,     // Center Back
    SW,     // Sweeper

    RWB,    // Right Wing Back
    LWB,    // Left Wing Back

    CDM,    // Defensive Midfielder
    CM,     // Central Midfielder
    CAM,    // Attacking Midfielder

    RM,     // Right Midfielder
    LM,     // Left Midfielder

    RW,     // Right Winger
    LW,     // Left Winger

    CF,     // Center Forward
    SS,     // Second Striker
    ST      // Striker
}
