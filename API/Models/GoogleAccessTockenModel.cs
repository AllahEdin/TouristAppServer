﻿using System.Collections.Generic;

namespace API.Models
{
	public class GoogleSuccessResultModel
    {
		public string access_token { get; set; }

		public int expires_in { get; set; }

		public string refresh_token { get; set; }

		public string scope { get; set; }

		public string token_type {get;set;}
		
		public string id_token { get; set; }
	}

	public class GoogleMetaInfo
	{
		public IEnumerable<GoogleNameInfo> names { get; set; }
	}

	public class GoogleNameInfo
	{
		public GoogleMetadata metadata { get; set; }

		public string displayName { get; set; }
	}

	public class GoogleMetadata
	{
		public GoogleUserSource source { get; set; }
	}

	public class GoogleUserSource
	{
		public string id { get; set; }
	}
}