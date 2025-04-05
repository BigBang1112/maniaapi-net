using MinimalXmlReader;

namespace ManiaAPI.Xml;

public delegate T XmlProcessContent<T>(ref MiniXmlReader xml);
