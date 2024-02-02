using DataManager;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Contact_Manager_UIX
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// The manager of the application
		/// </summary>
		private readonly Manager manager = new Manager();

		/// <summary>
		/// The selected folder item
		/// </summary>
		private TreeViewItem selectedFolder;

		/// <summary>
		/// The selected contact item
		/// </summary>
		private TreeViewItem selectedContact;

		/// <summary>
		/// The main setup of the application
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			contactRelation.SelectedIndex = 0;

			UpdateView();
		}

		/// <summary>
		/// Update the view of the treeview
		/// </summary>
		private void UpdateView()
		{
			treeView.Items.Clear();
			UpdateFolderView(manager.Root, treeView.Items);
		}

		/// <summary>
		/// Create a folder view
		/// </summary>
		/// <param name="folder">The folder</param>
		/// <param name="ic">The parent's view items</param>
		private void UpdateFolderView(Folder folder, ItemCollection ic)
		{
			TreeViewItem item = new TreeViewItem
			{
				Header = folder.Name,
				Tag = folder
			};
			item.Selected += SetCurrentFolder;
			item.KeyUp += DeleteFolder;
			item.IsExpanded = true;
			ic.Add(item);

			UpdateFolderContentView(item);
		}

		/// <summary>
		/// Create a contact view
		/// </summary>
		/// <param name="contact">The contact</param>
		/// <param name="ic">The parent's view items</param>
		private void UpdateContactView(Contact contact, ItemCollection ic)
		{
			TreeViewItem item = new TreeViewItem
			{
				Header = $"● {contact.Name} {contact.FirstName}",
				Tag = contact
			};
			ic.Add(item);
			item.Selected += SetCurrentContact;
			item.KeyUp += DeleteContact;
		}

		/// <summary>
		/// Generate the view of the folder's content
		/// </summary>
		/// <param name="item">The folder view</param>
		private void UpdateFolderContentView(TreeViewItem item)
		{
			if (item.Tag is Folder folder)
			{
				item.Items.Clear();

				foreach (Contact contact in folder.GetContacts())
				{
					UpdateContactView(contact, item.Items);
				}

				foreach (Folder subfolder in folder.GetFolders())
				{
					UpdateFolderView(subfolder, item.Items);
				}

				if (item == selectedFolder) folderModified.Text = folder.LastModified.ToString("G", CultureInfo.CurrentCulture);
			}
		}

		/// <summary>
		/// Triggered when the user press the delete key on a folder, delete the folder view
		/// </summary>
		/// <param name="sender">The folder view</param>
		/// <param name="e">The key event</param>
		private void DeleteFolder(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete) DeleteFolder((TreeViewItem)sender);
			e.Handled = true;
		}

		/// <summary>
		/// Delete the folder view
		/// </summary>
		/// <param name="item">The folder view</param>
		private void DeleteFolder(TreeViewItem item)
		{
			if (item.Tag is Folder folder)
			{
				selectedContact = null;

				if (folder == manager.Root)
				{
					manager.SetCurrentAsRoot();
					folder.Clear();
					UpdateFolderContentView(item);
				}
				else
				{
					selectedFolder = null;
					manager.Current = folder;
					manager.RemoveCurrent();
					UpdateFolderContentView((TreeViewItem)item.Parent);
				}
			}
		}

		/// <summary>
		/// Triggered when the user press the delete key on a contact, delete the contact view
		/// </summary>
		/// <param name="sender">The contact view</param>
		/// <param name="e">The key event</param>
		private void DeleteContact(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete) DeleteContact((TreeViewItem)sender);
			e.Handled = true;
		}

		/// <summary>
		/// Delete the contact view
		/// </summary>
		/// <param name="item">The contact view</param>
		private void DeleteContact(TreeViewItem item)
		{
			if (item.Tag is Contact contact)
			{
				selectedContact = null;
				manager.Current.RemoveItem(contact);
				UpdateFolderContentView((TreeViewItem)item.Parent);
			}
		}

		/// <summary>
		/// Triggered when the user select a folder, set the folder as the current folder
		/// </summary>
		/// <param name="sender">The folder view</param>
		/// <param name="e">The event</param>
		private void SetCurrentFolder(object sender, RoutedEventArgs e)
		{
			SetCurrentFolder((TreeViewItem)sender);
			e.Handled = true;
		}

		/// <summary>
		/// Set the folder as the current folder
		/// </summary>
		/// <param name="item">The folder view</param>
		private void SetCurrentFolder(TreeViewItem item)
		{
			if (item.Tag is Folder folder)
			{
				selectedFolder = item;
				selectedContact = null;
				manager.Current = folder;


				contactName.Text = contactFirstName.Text = contactCompany.Text = contactMailAddress.Text = "";
				contactRelation.SelectedIndex = 0;

				contactName.IsEnabled =
					contactFirstName.IsEnabled =
					contactCompany.IsEnabled =
					contactRelation.IsEnabled =
					contactMailAddress.IsEnabled =

					btnAddContact.IsEnabled =
					btnAddFolder.IsEnabled =
					btnClear.IsEnabled =
					btnRename.IsEnabled =

					folderName.IsEnabled =
					folderRename.IsEnabled = true;

				folderRename.Text = folder.Name;
				folderPath.Text = folder.GetPath();
				folderModified.Text = folder.LastModified.ToString("G", CultureInfo.CurrentCulture);
				folderCreated.Text = folder.CreationDate.ToString("G", CultureInfo.CurrentCulture);
				contactModified.Text = contactCreated.Text = "";
				btnAddContact.Content = "Ajouter";
			}
		}

		/// <summary>
		/// Triggered when the user select a contact, set the contact as the current contact
		/// </summary>
		/// <param name="sender">The contact view</param>
		/// <param name="e">The event</param>
		private void SetCurrentContact(object sender, RoutedEventArgs e)
		{
			SetCurrentContact((TreeViewItem)sender);
			e.Handled = true;
		}

		/// <summary>
		/// Set the contact as the current contact
		/// </summary>
		/// <param name="item">The contact view</param>
		private void SetCurrentContact(TreeViewItem item)
		{
			if (item.Tag is Contact contact)
			{
				SetCurrentFolder((TreeViewItem)item.Parent);
				selectedContact = item;

				btnAddContact.Content = "Modifier";

				if (contact == null)
				{
					contactModified.Text = contactCreated.Text = "";
					contactName.Text = contactFirstName.Text = contactCompany.Text = contactMailAddress.Text = "";
					contactRelation.SelectedIndex = 0;
				}
				else
				{
					contactName.Text = contact.Name;
					contactFirstName.Text = contact.FirstName;
					contactCompany.Text = contact.Company;
					contactRelation.SelectedIndex = (int)contact.Relation;
					contactMailAddress.Text = contact.MailAddress;
					contactCreated.Text = contact.CreationDate.ToString("G", CultureInfo.CurrentCulture);
					contactModified.Text = contact.LastModified.ToString("G", CultureInfo.CurrentCulture);
				}
			}
		}

		/// <summary>
		/// Triggered when the user press the add button, add or update the contact
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		public void AddOrUpdateContact(object sender, RoutedEventArgs e)
		{
			if (selectedContact?.Tag is Contact) UpdateContact();
			else AddContact();
		}

		/// <summary>
		/// Triggered when the user press the add button, add a folder
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		private void AddFolder(object sender, RoutedEventArgs e)
		{
			try
			{
				manager.AddFolder(folderName.Text);
				manager.ChangeCurrent("..");
				UpdateFolderContentView(selectedFolder);
			}
			catch (FormatException)
			{
				MessageBox.Show("Le nom du dossier ne peut pas être vide ou contenir '/' ou '\\'");
			}
			catch (ArgumentException)
			{
				MessageBox.Show("Le nom du dossier ne peut pas être vide");
			}
			catch (DuplicateNameException)
			{
				MessageBox.Show("Un dossier du même nom existe déjà");
			}
		}

		/// <summary>
		/// Add a contact
		/// </summary>
		private void AddContact()
		{
			try
			{
				manager.AddContact(contactName.Text, contactFirstName.Text, contactCompany.Text, (Relation)contactRelation.SelectedIndex, contactMailAddress.Text);
				UpdateFolderContentView(selectedFolder);
			}
			catch (FormatException)
			{
				MessageBox.Show("Adresse mail invalide");
			}
			catch (ArgumentException)
			{
				MessageBox.Show("Tout les champs doivent être remplis");
			}
		}

		/// <summary>
		/// Update a contact
		/// </summary>
		private void UpdateContact()
		{
			if (selectedContact.Tag is Contact contact)
			{
				try
				{
					contact.MailAddress = contactMailAddress.Text;
					contact.Name = contactName.Text;
					contact.FirstName = contactFirstName.Text;
					contact.Company = contactCompany.Text;
					contact.Relation = (Relation)contactRelation.SelectedIndex;
					contactModified.Text = contact.LastModified.ToString("G", CultureInfo.CurrentCulture);
					UpdateFolderContentView(selectedFolder);
				}
				catch (FormatException)
				{
					contactMailAddress.Text = contact.MailAddress;
					MessageBox.Show("Adresse mail invalide");
				}
				catch (ArgumentException)
				{
					MessageBox.Show("Tout les champs doivent être remplis");
				}
			}
		}

		/// <summary>
		/// Triggered when the user press the rename button, rename the folder
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		private void UpdateFolder(object sender, RoutedEventArgs e)
		{
			if (selectedFolder.Tag is Folder folder)
			{
				try
				{
					folder.Name = folderRename.Text;
					selectedFolder.Header = folder.Name;
					folderPath.Text = folder.GetPath();
					folderModified.Text = folder.LastModified.ToString("G", CultureInfo.CurrentCulture);
				}
				catch (FormatException)
				{
					MessageBox.Show("Le nom du dossier ne peut pas contenir '/' ou '\\'");
				}
				catch (ArgumentException)
				{
					MessageBox.Show("Le nom du dossier ne peut pas être vide");
				}
				catch (DuplicateNameException)
				{
					MessageBox.Show("Un dossier du même nom existe déjà");
				}
			}
		}

		/// <summary>
		/// Triggered when the user press the clear button, clear the folder
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		private void ClearFolder(object sender, RoutedEventArgs e)
		{
			if (selectedFolder.Tag is Folder folder)
			{
				folder.Clear();
				UpdateFolderContentView(selectedFolder);
			}
		}

		/// <summary>
		/// Triggered when the user press the load button, load from the save
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		private void Load(object sender, RoutedEventArgs e)
		{
			try
			{
				if (manager.Load(password.Password))
				{
					MessageBox.Show("Chargement effectué !");
					UpdateView();
				}
				else if (manager.TryCount <= 0)
				{
					MessageBox.Show("Mot de passe invalide, base existante supprimée");
				}
				else
				{
					MessageBox.Show($"Mot de passe invalide: {manager.TryCount} / 3 tentatives restantes");
				}
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("Aucune sauvegarde trouvée");
			}
		}

		/// <summary>
		/// Triggered when the user press the save button, save the data
		/// </summary>
		/// <param name="sender">The button</param>
		/// <param name="e">The event</param>
		private void Save(object sender, RoutedEventArgs e)
		{
			if (manager.Save(password.Password)) MessageBox.Show("Sauvegarde effectuée !");
			else MessageBox.Show("Erreur lors de la sauvegarde");
		}
	}
}
