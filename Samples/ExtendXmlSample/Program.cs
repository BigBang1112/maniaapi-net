using ExtendXmlSample;
using ManiaAPI.Xml;
using ManiaAPI.Xml.TMT;
using MinimalXmlReader;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TmEssentials;

var yourLogin = "zojytyxy-pc56f3bdff95566";
var yourSessionId = 2364832;
var platform = Platform.PC;

var platformLoginNicknamePairs = File.ReadLines("D:\\Visual Studio\\TurboLoginScraper\\TurboLoginScraper\\bin\\Debug\\net9.0\\pc.txt");

var initServer = new InitServerTMT(platform);

var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = new ExtendedMasterServerTMT(waitingParams.MasterServers.First());

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
jsonOptions.Converters.Add(new TimeInt32Converter());

var http = new HttpClient();

foreach (var line in platformLoginNicknamePairs.Skip(120))
{
    var parts = line.Split(' ');
    var login = parts[0];
    var nickname = parts[1];

    Console.WriteLine($"Processing {login} ({nickname})");

    var opponentRecords = await masterServer.GetChallengeRecordsComparisonAsync(login, yourLogin, yourSessionId);

    var requestInfo = JsonSerializer.Serialize(new
    {
        opponentRecords.Timestamp,
        opponentRecords.Login,
        opponentRecords.OpponentLogin
    }, jsonOptions);

    await File.WriteAllTextAsync(Path.Combine("PerLogin", $"{nickname} ({login}).json"), requestInfo);

    foreach (var rec in opponentRecords.Records)
    {
        var mapName = KnownMaps.ByUid[rec.MapUid];

        var perLoginDirPath = Path.Combine("PerLogin", $"{nickname} ({login})");
        var perMapDirPath = Path.Combine("PerMap", mapName);

        Directory.CreateDirectory(perLoginDirPath);
        Directory.CreateDirectory(perMapDirPath);

        var perLoginFilePath = Path.Combine(perLoginDirPath, $"{mapName}.Ghost.Gbx");
        var perMapFilePath = Path.Combine(perMapDirPath, $"{nickname} ({login}).Ghost.Gbx");

        Console.WriteLine($"Downloading {mapName} record ({rec.OpponentTimestamp})");

        await Task.Delay(200);

        Redownload:

        try
        {
            var ghostData = await http.GetByteArrayAsync(rec.OpponentDownloadUrl);

            await File.WriteAllBytesAsync(perLoginFilePath, ghostData);
            await File.WriteAllBytesAsync(perMapFilePath, ghostData);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine($"Failed to download {mapName} record: {ex.Message}");
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download {mapName} record: {ex.Message}");
            await Task.Delay(1000);
            goto Redownload;
        }

        var info = JsonSerializer.Serialize(rec, jsonOptions);

        await File.WriteAllTextAsync(Path.Combine(perLoginDirPath, $"{mapName}.json"), info);
        await File.WriteAllTextAsync(Path.Combine(perMapDirPath, $"{nickname} ({login}).json"), info);
    }
}

internal class ExtendedMasterServerTMT : MasterServerTMT
{
    public ExtendedMasterServerTMT(MasterServerInfo info) : base(info)
    {
    }

    public async Task<MasterServerResponse<RecordsComparison>> GetChallengeRecordsComparisonResponseAsync(string login, string authorLogin, int sessionId, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetChallengeRecordsComparison";
        return await RequestAsync($"<author><login>{authorLogin}</login><session>{sessionId}</session></author>",
            RequestName, $"<ol>{login}</ol>", (ref MiniXmlReader xml) =>
            {
                var mainTimestmap = default(DateTimeOffset);
                var login = string.Empty;
                var opponentLogin = string.Empty;
                var records = ImmutableList.CreateBuilder<RecordComparison>();

                while (xml.TryReadStartElement(out var element))
                {
                    switch (element)
                    {
                        case "t":
                            mainTimestmap = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                            break;
                        case "l":
                            login = xml.ReadContentAsString();
                            break;
                        case "ol":
                            opponentLogin = xml.ReadContentAsString();
                            break;
                        case "v":
                            var mapUid = string.Empty;
                            var score = default(TimeInt32);
                            var timestamp = default(DateTimeOffset);
                            var opponentScore = default(TimeInt32);
                            var opponentTimestamp = default(DateTimeOffset);
                            var opponentDownloadUrl = string.Empty;

                            while (xml.TryReadStartElement(out var subElement))
                            {
                                switch (subElement)
                                {
                                    case "c":
                                        mapUid = xml.ReadContentAsString();
                                        break;
                                    case "r":
                                        score = new TimeInt32((int)uint.Parse(xml.ReadContent()));
                                        break;
                                    case "d":
                                        timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                                        break;
                                    case "or":
                                        opponentScore = new TimeInt32((int)uint.Parse(xml.ReadContent()));
                                        break;
                                    case "od":
                                        opponentTimestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                                        break;
                                    case "ou":
                                        opponentDownloadUrl = xml.ReadContentAsString();
                                        break;
                                    default:
                                        xml.ReadContent();
                                        break;
                                }
                                _ = xml.SkipEndElement();
                            }

                            records.Add(new RecordComparison(mapUid, score, timestamp, opponentScore, opponentTimestamp, opponentDownloadUrl));
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                return new RecordsComparison(mainTimestmap, login, opponentLogin, records.ToImmutable());
            }, cancellationToken);
    }

    public async Task<RecordsComparison> GetChallengeRecordsComparisonAsync(string login, string authorLogin, int sessionId, CancellationToken cancellationToken = default)
    {
        return (await GetChallengeRecordsComparisonResponseAsync(login, authorLogin, sessionId, cancellationToken)).Result;
    }
}

internal sealed class TimeInt32Converter : JsonConverter<TimeInt32>
{
    public override TimeInt32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(TimeInt32));
        return new TimeInt32(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, TimeInt32 value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalMilliseconds);
    }
}

public static class KnownMaps
{
    public static Dictionary<string, string> ByName { get; } = new()
    {
        ["001"] = "rdClvxHy8JgOjrNrEbK4X5Jj7V",
        ["002"] = "4pQoOddIpzHQdtVNLcK3tvNvbo7",
        ["003"] = "GxU3iOGT7Smt_Zs77TZoL8rCCa8",
        ["004"] = "8sHSwzzzjNRUCH5w8T94sUfPZre",
        ["005"] = "7HR4Rv5hCUnOtrtuKSmVy67bcJ0",
        ["006"] = "_yG041avqwhMvYr7BptqG_o4ZE6",
        ["007"] = "rlHkbV9pSPBccx1z1HVk5DwrMri",
        ["008"] = "8M0gG9tU4iabde3J3prTbyM4wn7",
        ["009"] = "UPo3szSC7VUHvBQMPcaLA8P0nMl",
        ["010"] = "6Fy5gErdHMLqZLvHQj88L6XieVl",
        ["011"] = "eCcrPpKsfMIwzKk0MQc7BBUBot4",
        ["012"] = "UZ548IRpA1wX5Tf6Age9mnxg1Gm",
        ["013"] = "SJfPiBMcAZh3BQyZ39SrjzEjz61",
        ["014"] = "1vVNEWkhaxf6Pbd4IkgWTYimIc0",
        ["015"] = "H5ifnHfyB89K_bdSQQMLH8iAEDb",
        ["016"] = "f22AD7faeO4y9m_uoORV0AwKzag",
        ["017"] = "Qw7um9iQoXs1gcd6p6yyzdELwCm",
        ["018"] = "6uDWp2IKnHQFAyNDlqZR5N_dlQm",
        ["019"] = "5ocjGecH6ndTR19tsz_SoH29gXf",
        ["020"] = "UtnxtiGizDCVzfJP_392mfQ7mrc",
        ["021"] = "bjEvsdH1evcZyToEhqc2KVg0eoa",
        ["022"] = "4kJi4x9PTLAdiqM4fCfXPPB8sGg",
        ["023"] = "k_ldRH_nhe3CoUnV1ZhJLFdZk0i",
        ["024"] = "kuHckbBtz9V358dR3CzpP4hCqZ0",
        ["025"] = "9M8RyJrtVjDL3lXiK5NG530nXS6",
        ["026"] = "5sWBvjJfKr5IvtC5RWaSv8okTwe",
        ["027"] = "LpCyy9zinSGoReEUoyuEFKTWZ8e",
        ["028"] = "sCQNGGQsH7IskJ5hzbLTqvOdJ48",
        ["029"] = "HVgVpwJhQCpDk5h0moWcx8R0Shl",
        ["030"] = "Js4o1rScWjD2PQlUaABFLeUrbj8",
        ["031"] = "xcisw1p_aa6M5Tmp8uFcf3hHsph",
        ["032"] = "fDjbGVcMUxVtCREIsQ_tP6Cf8Yb",
        ["033"] = "henG2rvwIqldWA5xUXslzHqPNx2",
        ["034"] = "rxidB0ge8VfpNAzPw_Em2yU3Qz2",
        ["035"] = "s6L92Rb0swZ3Pp_URZybJu7OS6a",
        ["036"] = "0ykfSU0GfbpTQKiOcPjn6oWu73i",
        ["037"] = "AyDRoCpQz0yOpM_JstgHUgtKkx4",
        ["038"] = "Go_VlGNc_jz0cBLbFh4pwQEDIF9",
        ["039"] = "XJWjiRyWxaxq5kvFBSCSxy7D36k",
        ["040"] = "ktePhBgAaIRi5zle_D1tH9bF3el",
        ["041"] = "XUuBzeBgKAx0m6MMstYeTuWZGdb",
        ["042"] = "JxiYDFatjzBr808xEJ65_HnPeG0",
        ["043"] = "AcMRh6x4gYXUXaIb9CDnnER6tVg",
        ["044"] = "xLiIcdqfeDD0uFdt3GeggXEOkuj",
        ["045"] = "rLLj3npQ1D1cEMXZWbfXISizwJ4",
        ["046"] = "NrppF0G4ENEI9_2V627dPZb0Ii6",
        ["047"] = "RiJpMl2LotrqAO7IjGmIY2BOKHc",
        ["048"] = "8TyNQa52fxTJqu2_fW8OEWsxyrj",
        ["049"] = "KmIiEIrVpxz7vTwQH7HYiXzpd0e",
        ["050"] = "jupNQq_K0bdHpApmLTv0wXfbPU4",
        ["051"] = "s6dFCATJeTorAgaGx1GsAw87w7h",
        ["052"] = "qqO6v0Rvm81uUkEjgxBwLKe2V4b",
        ["053"] = "Y10UpwaCJveiJ5z9I0gf9Uezsff",
        ["054"] = "p3W9LavGCme1yzu59V41sCrNRUa",
        ["055"] = "W3jwefo33f3pBomGJCBaARYp1sj",
        ["056"] = "I8rEh72b5HUhz8i2KFZIQ8mFdbk",
        ["057"] = "xmKHAzTZ8HjrYvfmtU7erV0IBzc",
        ["058"] = "IRmdgd5tv7BTt8rE5t8RylH5gnh",
        ["059"] = "68CLItIO8_sqVsnl7qT5xG9Igra",
        ["060"] = "cMk5cGK5_nt_FNWBhHLbvioBKg7",
        ["061"] = "YK2ztzDHWI9qpOONtiLVLZPhqef",
        ["062"] = "phd2Q9GmnZq69LW2YiJp6MpzkNa",
        ["063"] = "0zHGaeC58PL1ksbwdORokdfm0I",
        ["064"] = "VXVtXk7dtIaRt54XtKWHcoBWyR3",
        ["065"] = "GxzE7f6ypuEPBtP6tDPg5qS5o_6",
        ["066"] = "2WigIMH1hGIJVnIWvfp9sT1UhJl",
        ["067"] = "TH0qjFPePDYjz3TT057I_JFUhm7",
        ["068"] = "thzM84G2xoepxyFXhfb5VmfDIB8",
        ["069"] = "jawsnMJtJHjgZN4YOlZuaBrRbke",
        ["070"] = "s5Uu8nrmePxGLkrVkU8TL8uTI63",
        ["071"] = "ncYEHjNezVgxx9DZp8DSrckBlH2",
        ["072"] = "gKf0qZfMpc8Cd3ZxCVgzBmjGtM3",
        ["073"] = "LsPtRMnE_Or73URMNncjoO2mgjd",
        ["074"] = "BnAOBU_K39tq5htTW3ZgT3BMuIb",
        ["075"] = "2QTNxKnWoZAF7s_8_575laDM9X0",
        ["076"] = "Vkvy2fdI2uKPORxZc9G6xJk8i0f",
        ["077"] = "IKqb2swC6CcHFVatZHrgFDNl_X6",
        ["078"] = "9hHto2CJxyJnJyzPNrRpzdxw4Z9",
        ["079"] = "VezskkwkOd3v3hdmIsTPLHxRqJ5",
        ["080"] = "W5yu8oAEW4mGHyTw43lSBNPH4pi",
        ["081"] = "9iktGoDZvhqgc_iYLcr9Kkx8E26",
        ["082"] = "EcChHIU2xYtOssAfnDF_spESSj0",
        ["083"] = "25DU78NJvWNDoGL1UDCFhah3NIh",
        ["084"] = "NpN_t0M_LwwXu8vDpXDO1QlPmC8",
        ["085"] = "2HfsNUIdzA2guwQNaB7iSLrfl4n",
        ["086"] = "_EgvFFkMj9gCeQ1QbxXuRK41ymb",
        ["087"] = "otZWINTIlOi8BvYd_AzSJuF9IJi",
        ["088"] = "hnba3puzPxrQUojy6yx4szecsRf",
        ["089"] = "9ikInR9LycTjz5zc7XcI19RzhTj",
        ["090"] = "aF7sHBuQQJu4rlura8233lPeCS",
        ["091"] = "pi6_xX462vMw9fgGYh2wL3W1_39",
        ["092"] = "aLoHM7GbsVUvHnqda_4_MKGlZK8",
        ["093"] = "2Cv5wxzkKW4MFnF3RmxHiV7Iv4n",
        ["094"] = "KyjT3_e9DsTPYUUnF3COxzIXNB",
        ["095"] = "VMd5bbXY58N3fm_WwHJor4iNTb1",
        ["096"] = "PAgMsWjqkZaNw91Rnu6WuvNt462",
        ["097"] = "aOpJSZ9Yoxh13tEjXzyKax4UAQ5",
        ["098"] = "CboRVghJtzOcAgjUfXRF1w7vKbj",
        ["099"] = "iY1GWl69R9BssG64t5tyNG0FtR8",
        ["100"] = "VnA7wB11l7UjWYZ5uZGkuNK1_d5",
        ["101"] = "TDQAaKRbvHzDx4ZmgjWre5v9nke",
        ["102"] = "qKsesW_pHxeyjJcmksW9lp_YSFk",
        ["103"] = "eEzasoebsyEdq3_o9PHhCMXT3p5",
        ["104"] = "UqclUkIC1ITaOyKj4JdwBEJod1d",
        ["105"] = "Ye_5gMpzhv9hG9j2a4E6EKmTbq5",
        ["106"] = "FS0m32BradjHWWbKkLdHpX7YVkm",
        ["107"] = "AciwOlVsLPm7j_y4cr17cJw1yfh",
        ["108"] = "Zq4c6rhDBY9RjOj0k5lVQSQRH7i",
        ["109"] = "oCAbM52wUJDdkvRax1Xs8RJXawa",
        ["110"] = "Q2a4rGOTCBy6_EdbY4mz7nEEKsb",
        ["111"] = "WfjkN4bMFIYLRzXiQ6KCKsqG8T8",
        ["112"] = "dKbIOFSXb9lUAPP0CnadyOzvBN8",
        ["113"] = "7EGpoJtHofcOpM39m0fAa4oucA3",
        ["114"] = "kyNTF8rUiawHEdr0QDhGC5b_ldb",
        ["115"] = "7BiKpnhW7BgNNucDgIAnFighkFa",
        ["116"] = "09AzYGtROh3KKMGwgwP2kCDIgam",
        ["117"] = "CMPwpo6HyuVQF_Gc6XsTm9bckJl",
        ["118"] = "vRgJ2dDyan8TWsZ_S9hSj6Ec43k",
        ["119"] = "_6xGeek0C7AI1LwKjwK1BpTIbKb",
        ["120"] = "W4iaUP7IyYxcbDOjSXYVjs9On5g",
        ["121"] = "tKBMYOyD3Rapb8RDlJYdpx8APn4",
        ["122"] = "1R0bP4J39jbsO8gNlY3FdLUCcF7",
        ["123"] = "6kdKGzEKMWr5WxzHWalcMIxluxc",
        ["124"] = "jAAKFqE6TaWBEs46CxPrRIUxoVb",
        ["125"] = "Xs9DrZBjJRBJxB0u3A6r2oBZFI2",
        ["126"] = "M6nANSdZpC4vw4g0gycc_S0AWsg",
        ["127"] = "j5zlqFpy8Lown4u0nNc6SwJd8Ic",
        ["128"] = "v_s57SqzQsCA12W5zobKGE4EQRj",
        ["129"] = "HO_Zz7870O01sZAlJHzEsEwuXDi",
        ["130"] = "2YGj98HUSuraSUO9Mvkn5WxIIea",
        ["131"] = "95GEEYvptuvUUeB80veyvItKFkm",
        ["132"] = "HVJkqdqkiCO6H6R_bDBm86Ndyog",
        ["133"] = "IQDPlWiPHbYfoH7LpU7j3qw16hg",
        ["134"] = "7X_JzSrO96GImIpjyacthDRymok",
        ["135"] = "N7Ex1NP45Q6cBD_NYkfzSf7X0sl",
        ["136"] = "7HP1tdkXl8ZMLGzw3csSkLVCsni",
        ["137"] = "GZaxZgUMMRkj7VNnSetTPp4ccTk",
        ["138"] = "Xr_g62st9RYzdspFFBU_upTg3Yh",
        ["139"] = "I6Po_3kuiSpwoihp30JLERM5B9g",
        ["140"] = "3SHnlC14e5tS_bA7RWvqawear_e",
        ["141"] = "sMUGuODgGHV34seWn_kD5EUcs2c",
        ["142"] = "rig31CU2M3VKnLPiq1C4I85zPkc",
        ["143"] = "AbgK02nTIcpUcsN0JSOxDRPUCGk",
        ["144"] = "lKGP2Ww2uZpI_xBZH5gyHYYgzX8",
        ["145"] = "GZ26lCDCvjgBRj6d_ANl4fE5Iqf",
        ["146"] = "1J_nideio5QbS3MbCVR8_yiDEK4",
        ["147"] = "LRW7aJL2ZnupWCQfaGKBHCycbFh",
        ["148"] = "j_YiZs_L0T9BXoIs1hLggtQCff",
        ["149"] = "FxN8Yy1CHKMCmrNeGrELTVYJ2Vd",
        ["150"] = "oqrrUzNlc3S6wz90XIt7qUl9282",
        ["151"] = "OnGXOutJa6qgUs9HUt2lieWuqSd",
        ["152"] = "CA3gAh1GsPIiiYjX7WpkSSXVdH2",
        ["153"] = "xypQ7MiV05aoZYT1vL0iPCoEeWb",
        ["154"] = "bM8rh9VI1pbXEek3x97PakR7sGd",
        ["155"] = "3Nmo8NclUVIpuArHee1OJXd7q20",
        ["156"] = "krRxqqA4Eh1iwKShrn5vRJ0M8H9",
        ["157"] = "pi0FmvZmr4FT0EkL6fE34ZRZPVm",
        ["158"] = "qDpi8mz5P2eCNVOkfvisn9Z_M9c",
        ["159"] = "iIWN7PexPNxhGhCKkVmk9j3pNYk",
        ["160"] = "dArDGtfYQPte819UFA_35c7Jpgh",
        ["161"] = "EH4nf8VXv6fAXLdhGYOQz1LM3Tk",
        ["162"] = "FMieDDGOAwpXVisGRaay18HPu70",
        ["163"] = "KyAXpFpXuUVDZQncN24VLniyQH0",
        ["164"] = "FhcfV_F66ez3v9zTIPJtZz_Qxte",
        ["165"] = "fDUySVC83qgtyKd3tAKbyrUTCyf",
        ["166"] = "eho1V0npoKPbLZMeqhT20WEXcx0",
        ["167"] = "FIweya9BFNlLe4QayGvDZZeOJhj",
        ["168"] = "0nJCC65KDq8wEnu9r488Z4KdZXd",
        ["169"] = "MJHMdiyUVC68MNPVV5hwWE6CQX",
        ["170"] = "ipm7ukiOrMineS1aHIxghCfkDA7",
        ["171"] = "LyfHT_vqdDTeYPHlVeqcK0orbj1",
        ["172"] = "bmJW8IhHoQiYAwoQW81chlQAr09",
        ["173"] = "LsdRidAAMcDiipSpjRw1GuSRkr7",
        ["174"] = "dCDzG1quVV66HoXU5kgsWNeML0l",
        ["175"] = "F2X_qJtLTsr_Ybk7hSk7Vpt2_p9",
        ["176"] = "Cj4TJm7Je2zJFxce3VArh35c35f",
        ["177"] = "f_6_mQuDIqNeJLAlnIAlC9c45Og",
        ["178"] = "uksTK9yxnXdE7aKnwKhJeoxYK7l",
        ["179"] = "1SGJVgDg63R4pE3XWKrUsbc96Cb",
        ["180"] = "Gdpm02uXTvSLhzbA2yAF3MGyrR7",
        ["181"] = "tdSLUTsyERwEz5_kF89jZob1dnb",
        ["182"] = "PwqKtpp9AeWdklb2e6n1FHi_Qyf",
        ["183"] = "o_yd_otJJyiMXYJ1FTWCbPpVOPm",
        ["184"] = "dZrC7tbSLf_gd9SLuVZlZoHYyKi",
        ["185"] = "ub8B4iFEg6u7bXEuXOJQKxkcaVa",
        ["186"] = "BBJ3GDjzvFRWOaD6lDSN1OfGnv",
        ["187"] = "fDgNq0jTJJe_9uOwa3INVr1zGR9",
        ["188"] = "VFiGxwPZT__VymNhHvzC7lSKsDc",
        ["189"] = "CyzK3bS5J6lCY_RvQMORI4DPYvm",
        ["190"] = "Ouw0TlRpJ7VSBmK1FgQowyVTAab",
        ["191"] = "yl0KhVnZE_mrmnFzGaIX4XLpw42",
        ["192"] = "zCi98ja74OipnR5GwwLokbV_nf1",
        ["193"] = "UpYkn8JFC5kioxDgK9clHp_hERd",
        ["194"] = "XCfmmnX_M_ctxfHt6anW_Ytukx5",
        ["195"] = "C6QTj4DgTst8hekMPxMG3Sr9TF3",
        ["196"] = "dTDcCij9duFSlYgAycE__0VkHu1",
        ["197"] = "Iw89lemUd0HBXx2RAcOGiabSqm6",
        ["198"] = "alILbsbtcdzR1bYJDDd36n26MGm",
        ["199"] = "gXrH4ISiKX2nqFLn7xKdwe8u369",
        ["200"] = "jV1HYr_Vp3uijy8jgaC5TkHkRs",
        ["VR_Canyon_001"] = "6qfYPwrZfJSyZ5P4N_SSCzrM5D1",
        ["VR_Canyon_002"] = "0rkE_2a2A1ATloM_9OriY0KfZY5",
        ["VR_Canyon_003"] = "kta4s4jrqcz3VkMBkawNnLFrHL",
        ["VR_Canyon_004"] = "U9CNw6WlbUnHClmRZ48fgGZ6Rdb",
        ["VR_Canyon_005"] = "OXrjvghIs3flvUq0U2wueR27NB4",
        ["VR_Canyon_006"] = "_4RmqXIu2Ux0xTFeyEkee6tbv8i",
        ["VR_Canyon_007"] = "u080sxlw2MUZQg2PYUQIkH33Phd",
        ["VR_Canyon_008"] = "gGRPAVhKyx2aN1Uejd71BksuBu8",
        ["VR_Canyon_009"] = "btebfo7VYCqYwB9y1uoB7miA54f",
        ["VR_Canyon_010"] = "O7yTkZEhS7qxo1rit0EfataCX5f",
        ["VR_Valley_001"] = "3VpPdHY4FPa2wFCfdoORbiQt5b4",
        ["VR_Valley_002"] = "RcZDQda8JZasxd22EHwDR8G_yza",
        ["VR_Valley_003"] = "oapwaVI7tUteiHYc7l0NHSQB6zl",
        ["VR_Valley_004"] = "w2dKl8inwhK3Bv0_OEMbyIBE9Fa",
        ["VR_Valley_005"] = "AA4rpP9DgMYoegRSYphg_jXvhC2",
        ["VR_Valley_006"] = "7jFADCqi235vpS8Km3dcz62Hc5e",
        ["VR_Valley_007"] = "oIIlAiQ8CZMKlyh6UGneAX1W9Pb",
        ["VR_Valley_008"] = "5ObQdf5wZ2dpOHE8EV_uCWOyKJh",
        ["VR_Valley_009"] = "MCkrCVcPh4veQk7AbouzXqJsdLm",
        ["VR_Valley_010"] = "62R2MtSPY8MA00f9cfyBv_ORUT5",
        ["VR_Lagoon_001"] = "JKDJGU_4YKPDG8BvUtyhCW9Uzbb",
        ["VR_Lagoon_002"] = "VYRoDwcbe3qWV6AjbqcIvG4i7y2",
        ["VR_Lagoon_003"] = "K4fRZsTdU9DCumoQqV73nNhKPkh",
        ["VR_Lagoon_004"] = "32ns7fR5nwSk9wIrEEETiAhfeKb",
        ["VR_Lagoon_005"] = "_UryRQLB5Wv5gr6pCIGg4cRbtTf",
        ["VR_Lagoon_006"] = "w2StQPhDLaS7E2JcIKlSlnOgH33",
        ["VR_Lagoon_007"] = "_Yanf7Wom9jnw46CsnCtgUMfiv3",
        ["VR_Lagoon_008"] = "py3uUei0vU8ch8g_YhSLE6wdw_0",
        ["VR_Lagoon_009"] = "DDyHlEw8as6N5nIUCPpbqJt81Oa",
        ["VR_Lagoon_010"] = "GaXdQyv54PURmK7CxtflpDKcYCl",
        ["VR_Stadium_001"] = "Q8B8qCCeGWzsen1ivq9uTDjMyGk",
        ["VR_Stadium_002"] = "fYyEIdw_Wrp_MlKwwqEdNS3fmGd",
        ["VR_Stadium_003"] = "DtR0zOjUJdkyiEXz7VJqSJmPgXc",
        ["VR_Stadium_004"] = "sMRJJ5WwxiUZZPSgTvR6S7SXFIj",
        ["VR_Stadium_005"] = "KQhVEzhAsa6xSLmXPMXlHJaAolb",
        ["VR_Stadium_006"] = "z3WoZc_9KXeFfDHhoCp1hx6Ql0n",
        ["VR_Stadium_007"] = "0pL29o4HDcQsTQU6hjGC82xdLY2",
        ["VR_Stadium_008"] = "3fSeq2hAXXnh7eCEiPw2zdseHB6",
        ["VR_Stadium_009"] = "13pe5bqBS2Vt_Kv7FW4e1jxOvil",
        ["VR_Stadium_010"] = "DuUbu3Qcs_gEflW1c50AuFxVnxc"
    };

    public static Dictionary<string, string> ByUid { get; } = ByName.ToDictionary(kv => kv.Value, kv => kv.Key);
}