using MinimalXmlReader;

namespace ManiaAPI.XmlRpc;

public delegate T XmlRpcProcessContent<T>(ref MiniXmlReader xml);
