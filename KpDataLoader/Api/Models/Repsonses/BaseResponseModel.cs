﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpDataLoader.Api.Models.Repsonses
{
    public abstract class BaseResponseModel: IResponseModel
    {
        public int StatusCode { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
