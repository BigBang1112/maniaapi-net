using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ManiaAPI.NadeoAPI.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record SubmittedMap([property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                  bool? Rejected,
                                  MapInfoLive? Map,
                                  int? DifficultyScore,
                                  int? MScore,
                                  int? PlayerCountAtStart,
                                  bool? Selected,
                                  int? PlayerFinishCount,
                                  string MapUid,
                                  bool Nominated,
                                  MapReviewNoteInfo? NoteInfo,
                                  ImmutableList<string> MapStyles,
                                  int NadeoNote,
                                  ImmutableList<string> Labels,
                                  int? DifficultyCount,
                                  int? PlayerCountAtEnd,
                                  int FeedbackCount,
                                  int? PlayerDidntFinishCount,
                                  int? MapReviewId,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset LatestSubmissionTimestamp,
                                  int? TimePlayed,
                                  bool MessagingOpen,
                                  int? MScoreExperimental);
