
# Osmium::AuthN

unified authentication server for AlphaOsmium

## Flow

- Server maintains user accounts in database
- Upon authentication, client receives a signed JWT blob containing claims:
	- username, identifier, groups (a list of strings)
- 	
