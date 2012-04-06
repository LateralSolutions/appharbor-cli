﻿using System;
using System.Linq;
using AppHarbor.Model;

namespace AppHarbor
{
	public class GitRepositoryConfigurer : IGitRepositoryConfigurer
	{
		private readonly IFileSystem _fileSystem;
		private readonly IGitCommand _gitCommand;

		public GitRepositoryConfigurer(IFileSystem fileSystem, IGitCommand gitCommand)
		{
			_fileSystem = fileSystem;
			_gitCommand = gitCommand;
		}

		public void Configure(string id, User user)
		{
			var repositoryUrl = string.Format("https://{0}@appharbor.com/{1}.git", user.Username, id);

			try
			{
				_gitCommand.Execute("--version");
			}
			catch (GitCommandException)
			{
				throw new RepositoryConfigurationException(string.Format("Git is not installed."));
			}

			try
			{
				_gitCommand.Execute("status");
			}
			catch (GitCommandException)
			{
				Console.Write("Git repository is not initialized in this folder. Do you want to initialize it (type \"y\")?");
				if (Console.ReadLine() != "y")
				{
					throw new RepositoryConfigurationException("Git repository was not initialized.");
				}

				_gitCommand.Execute("init");
				Console.WriteLine("Git repository was initialized.");
			}

			try
			{
				_gitCommand.Execute(string.Format("remote add appharbor {0}", repositoryUrl));

				Console.WriteLine("Added \"appharbor\" as a remote repository. Push to AppHarbor with git push appharbor master.");
			}
			catch (GitCommandException)
			{
				throw new RepositoryConfigurationException(
					string.Format("Couldn't add appharbor repository as a git remote. Repository URL is: {0}.", repositoryUrl));
			}
		}

		public string GetApplicationId()
		{
			var url = _gitCommand.Execute("config remote.appharbor.url").FirstOrDefault();
			if (url == null)
			{
				throw new RepositoryConfigurationException();
			}

			return url.Split(new string[] { "/", ".git" }, StringSplitOptions.RemoveEmptyEntries).Last();
		}
	}
}