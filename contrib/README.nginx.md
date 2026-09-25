# Vhosts nginx — génération par `make generate-nginx`

Les vhosts nginx sont **générés** à l'image des unités de service systemd
(`template.service` + `envsubst` → `generated/*.service`) : un template
unique `contrib/template.vhost` est substitué par `envsubst` vers
`contrib/generated/nginx/`, qui est **gitignoré** (comme `generated/`).

HTTP/3 (QUIC) **termine au edge nginx** sur 443/udp. Kestrel reste en
h1/h2 sur son port backend ; nginx lui parle en HTTP/1.1
(`proxy_http_version 1.1`).

## Génération

```bash
cd contrib

# Preprod (défaut) -> generated/nginx/preprod-yavsc{Org,Blogs,Api}
make generate-nginx

# Production -> generated/nginx/yavsc{Org,Blogs,Api}
make generate-nginx-prod
# ou manuellement :
make generate-nginx APP_INSTALL_ENV=production APP_INSTALL_PREFIX=
```

Les ports backend et `HTTP_HOST` proviennent de `.preprod.env` /
`.production.env` (ex. prod Org=83, Api=87, Blogs=85 ; preprod 89/93/91).
Le schéma backend est `https` en prod (Kestrel écoute HTTPS, cert
Let's Encrypt) et `http` en preprod (Kestrel écoute HTTP simple).

| Hôte  | Sous-domaine | `reuseport` | Backend prod | Backend preprod |
|-------|--------------|-------------|--------------|-----------------|
| Org   | yavsc        | **oui**     | `https://localhost:83` | `http://localhost:89` |
| Api   | api          | non         | `https://localhost:87` | `http://localhost:93` |
| Blogs | blogs        | non         | `https://localhost:85` | `http://localhost:91` |

Domaines : `<prefix><subdomain>.pschneider.fr`
(prod : `yavsc.pschneider.fr` ; preprod : `preprod-yavsc.pschneider.fr`).

## Déploiement sur le host

```bash
# depuis le host, après avoir copié les fichiers générés :
sudo cp generated/nginx/yavscOrg  /etc/nginx/sites-available/yavscOrg
sudo ln -sf /etc/nginx/sites-available/yavscOrg /etc/nginx/sites-enabled/yavscOrg
# (idem pour yavscApi, yavscBlogs)
sudo nginx -t && sudo systemctl reload nginx
```

## Prérequis côté host

1. **nginx ≥ 1.25** compilé avec `http_v3_module` :
   ```bash
   nginx -V 2>&1 | grep -o 'http_v3_module'   # doit être non vide
   ```
   Debian 13 « trixie » fournit `nginx` avec `http_v3_module`. Sinon,
   `nginx-full` ou les backports.
2. **UDP 443 ouvert** :
   ```bash
   sudo ufw allow 443/udp
   sudo ufw status | grep '443/udp'           # → ALLOW
   ```
3. Certbot / certs Let's Encrypt déjà en place (réutilisés tels quels —
   les chemins `/etc/letsencrypt/live/<domain>/{fullchain,privkey}.pm`
   sont référencés par le template).

## Règle : un seul `reuseport` sur 443/quic

`listen 443 quic reuseport;` ne peut figurer **qu'une seule fois** par
couple adresse:port, pour tous les vhosts confondus. Le template le
pose sur **Org** seulement (`QUIC_OPTS=reuseport` pour Org, vide pour
Api/Blogs — règle `$(if $(filter Org,$*),reuseport,)` dans le
`Makefile`). Si vous réordonnez les vhosts, gardez `reuseport` sur un
seul.

## Vérification (sur le host)

```bash
# 1. Module h3 présent
nginx -V 2>&1 | grep -o 'http_v3_module'

# 2. UDP 443 ouvert
sudo ufw status | grep '443/udp'

# 3. En-tête Alt-Svc annoncé
curl -sI https://yavsc.pschneider.fr/ | grep -i alt-svc   # → h3=":443"

# 4. Requête HTTP/3 effective (curl doit être compilé avec HTTP/3)
curl -v --http3 https://yavsc.pschneider.fr/ 2>&1 | grep -i 'HTTP/3'

# 5. Fallback h2 et h1 toujours fonctionnels
curl --http2   https://yavsc.pschneider.fr/ -o /dev/null -w '%{http_code}\n'
curl --http1.1 https://yavsc.pschneider.fr/ -o /dev/null -w '%{http_code}\n'
```

Répéter 3-5 pour `api.pschneider.fr` et `blogs.pschneider.fr` (prod),
et pour les trois domaines preprod. Sans curl HTTP/3, utiliser un
navigateur : DevTools → Network → colonne « Protocole » affiche `h3`
après la première mise à niveau.

## Comment ça marche (template + envsubst)

`contrib/template.vhost` contient des placeholders `$DOMAIN`,
`$BACKEND_SCHEME`, `$HTTP_HOST`, `$BACKEND_PORT`, `$QUIC_OPTS`. La
règle `generated/nginx/$(APP_INSTALL_PREFIX)yavsc%` du `Makefile` les
substitue via `envsubst` avec une **allowlist** :

```makefile
envsubst '$DOMAIN $BACKEND_SCHEME $HTTP_HOST $BACKEND_PORT $QUIC_OPTS'
```

Cette allowlist est **critique** : elle limite la substitution aux 5
variables citées, de sorte que les variables nginx intrinsèques
(`$host`, `$scheme`, `$remote_addr`, `$proxy_add_x_forwarded_for`,
`$request_uri`) survivent intactes dans le vhost généré. Sans
allowlist, `envsubst` détruirait ces variables nginx.

## Hors périmètre

- **Kestrel inchangé** : ni `Protocols: Http1AndHttp2AndHttp3`, ni
  `libmsquic`. Si l'on veut plus tard que Kestrel parle aussi h3
  (accès direct sans nginx / topo L4), ajouter
  `Kestrel:Endpoints:Https:Protocols = "Http1AndHttp2AndHttp3"` dans
  les `appsettings-{app}*.json` + `apt install libmsquic` sur le host
  (Kestrel désactive h3 silencieusement si libmsquic absent — fallback
  gracieux).
- **docker-compose / Dockerfile** : non touchés (déploiement réel
  bare-metal systemd + nginx).
- **Backend HTTP/2** : nginx open-source ne supporte que HTTP/1.1 en
  upstream ; on garde `proxy_http_version 1.1`.