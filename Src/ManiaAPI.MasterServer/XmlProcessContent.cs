using MinimalXmlReader;

namespace ManiaAPI.MasterServer;

public delegate T XmlProcessContent<T>(ref MiniXmlReader xml);
