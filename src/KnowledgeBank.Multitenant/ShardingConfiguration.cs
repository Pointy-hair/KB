namespace KnowledgeBank.Multitenant
{
	public class ShardingConfiguration
	{
		/// <summary>
		/// Gets or Sets the Name of the ShardMap.
		/// </summary>
		public string ShardMap { get; set; }
		/// <summary>
		/// Gets or Sets the prefix for the database name of the shard.
		/// </summary>
		public string ShardPrefix { get; set; }
		/// <summary>
		/// Gets or Sets the server the shard will reside on.
		/// </summary>
		public string ShardServer { get; set; }
		/// <summary>
		/// Gets or Sets the shard connection credentials (eg:"Trusted_Connection=True;")
		/// </summary>
		public string ConnectionCredentials { get; set; }
		/// <summary>
		/// Gets or Sets the name of the elastic pool new shard shoul be asigned to.
		/// </summary>
		public string ElasticPool { get; set; }
		/// <summary>
		/// Gets the Server and the credentials to the shard
		/// </summary>
		public string ServerConnection => $"Server={ShardServer};{ConnectionCredentials}";
		/// <summary>
		/// Gets or Sets the Database Principal (User from the connection string)
		/// </summary>
		public string DatabasePrincipal { get; set; }
		/// <summary>
		/// Gets or Sets the Database SuperUser (cann acess data of all tenants)
		/// </summary>
		public string DatabaseSuperUser { get; set; }
	}
}
