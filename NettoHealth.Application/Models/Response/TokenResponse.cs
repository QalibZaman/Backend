using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoHealth.Application.Models.Response;

public record TokenResponse(string AccessToken, string RefreshToken);
