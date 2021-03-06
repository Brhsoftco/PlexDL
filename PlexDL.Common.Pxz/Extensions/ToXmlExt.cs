﻿using PlexDL.Common.Pxz.Structures;
using System.Xml;

namespace PlexDL.Common.Pxz.Extensions
{
    public static class ToXmlExt
    {
        public static XmlDocument ToXml(this PxzIndex obj)
        {
            return Serializers.PxzIndexToXml(obj);
        }
    }
}