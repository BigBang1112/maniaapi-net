using MinimalXmlReader;

namespace ManiaAPI.XmlRpc;

internal delegate T XmlRpcProcessContent<T>(ref MiniXmlReader xml);
