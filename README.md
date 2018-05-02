
# Osmium::AuthN

unified authentication server for AlphaOsmium

## Flow

- Server maintains user accounts in database
- Upon authentication, client receives a signed JWT-like blob containing claims:
	- a payload containing username, identifier, groups (a list of strings), in base64-encoded JSON
	- cryptographic signature (RSA with SHA384) in base64
- Applications store a list of auth server public keys that are trusted to verify accounts
- Application parses the payload, and verifies the signature to ensure integrity
	- the authentication information can be used to create a "federated id" of the form user@serverId
