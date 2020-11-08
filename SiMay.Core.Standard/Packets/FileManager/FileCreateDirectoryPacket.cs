﻿using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileCreateDirectoryPacket : EntitySerializerBase
    {
        public string DirectoryName { get; set; }
        public bool NoCallBack { get; set; }
    }

    public class FileCreateDirectoryFinishPack : EntitySerializerBase
    {
        public bool IsSuccess { get; set; }
    }
}