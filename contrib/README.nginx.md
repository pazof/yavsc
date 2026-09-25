# Vhosts nginx — génération par `make generate-nginx`

Les vhosts nginx sont **générés** à l'image des unités de service systemd
(`template.service` + `envsubst` → `generated/*.service`) : un template
unique `contrib/template.vhost` est substitué par `envsubst` vers
`contrib/generated/nginx/`, qui est **gitignoré**.

## Rôle de nginx : edge HTTP/3 (plan `contrib/quic.md`)

nginx termine HTTP/3 (QUIC) au edge sur 443/udp, en coexistence avec
h1/h2 sur 443/tcp. Kestrel reste backend h1/h2 (`proxy_http_version 1.1`).
Le « QUIC logiciel » Kestrel (`Protocols: Http1AndHttp2AndHttp3` dans les
`template.appsettings-*.json`) est une capacité complémentaire — non
joignable par les clients derrière nginx (nginx OSS = upstream h1.1
uniquement), mais disponible si Kestrel est un jour exposé en direct.

Directives générées (par vhost) :
```nginx
listen 443 ssl;          listen [::]:443 ssl;          http2 on;
listen 443 quic $QUIC_OPTS;   listen [::]:443 quic $QUIC_OPTS;   http3 on;
add_header Alt-Svc 'h3=":443"; ma=86400' always;
ssl_protocols TLSv1.3;
```
`$QUIC_OPTS = reuseport` sur **Org uniquement**, vide sur Api/Blogs (règle
`contrib/quic.md` : `reuseport` une seule fois par addr:port, tous vhosts
confondus).

## Prérequis côté host

1. **nginx avec `http_v3_module`** (≥ 1.25 ; la directive `http3 on;`
   exige ≥ 1.27.4 — sur une version 1.25–1.27.3, retirer `http3 on;`,
   `listen ... quic` suffit à activer h3) :
   ```bash
   nginx -V 2>&1 | grep -o 'http_v3_module'   # doit être non vide
   ```
   Debian 13 « trixie » fournit `nginx` avec `http_v3_module`. Sinon,
   backports ou `nginx-full`.
2. **UDP 443 ouvert** :
   ```bash
   sudo ufw allow 443/udp
   sudo ufw status | grep '443/udp'           # → ALLOW
   ```
3. **IPv6** : le template émet `listen [::]:443 ...` (et `[::]:80`). Si le
   host n'a pas d'IPv6, retirer/commenter ces lignes (sinon `nginx -t`
   échoue sur le bind IPv6).
4. Certbot / certs Let's Encrypt déjà en place (chemins
   `/etc/letsencrypt/live/<domain>/{fullchain,privkey}.pem` référencés).
5. (Optionnel, pour le QUIC logiciel Kestrel) `apt install libmsquic` —
   sinon Kestrel désactive h3 silencieusement (fallback h1/h2 gracieux).

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

Ports backend et `HTTP_HOST` depuis `.preprod.env` / `.production.env`
(prod Org=83, Api=87, Blogs=85 ; preprod 89/93/91). Schéma backend
`https` en prod, `http` en preprod.

| Hôte  | Sous-domaine | `reuseport` | Backend prod            | Backend preprod        |
|-------|--------------|-------------|-------------------------|------------------------|
| Org   | yavsc        | **oui**     | `https://localhost:83`  | `http://localhost:89`  |
| Api   | api          | non         | `https://localhost:87`  | `http://localhost:93`  |
| Blogs | blogs        | non         | `https://localhost:85`  | `http://localhost:91`  |

Domaines : `<prefix><subdomain>.pschneider.fr`
(prod `yavsc.pschneider.fr` ; preprod `preprod-yavsc.pschneider.fr`).

## Déploiement sur le host

```bash
sudo cp generated/nginx/yavscOrg  /etc/nginx/sites-available/yavscOrg
sudo ln -sf /etc/nginx/sites-available/yavscOrg /etc/nginx/sites-enabled/yavscOrg
# (idem yavscApi, yavscBlogs)
sudo nginx -t && sudo systemctl reload nginx
```

## Vérification (sur le host)

```bash
# 1. Module h3 présent
nginx -V 2>&1 | grep -o 'http_v3_module'

# 2. UDP 443 ouvert
sudo ufw status | grep '443/udp'

# 3. En-tête Alt-Svc annoncé
curl -sI https://yavsc.pschneider.fr/ | grep -i alt-svc   # → h3=":443"

# 4. Requête HTTP/3 effective (curl compilé avec HTTP/3)
curl -v --http3 https://yavsc.pschneider.fr/ 2>&1 | grep -i 'HTTP/3'

# 5. Fallback h2 et h1 toujours fonctionnels
curl --http2   https://yavsc.pschneider.fr/ -o /dev/null -w '%{http_code}\n'
curl --http1.1 https://yavsc.pschneider.fr/ -o /dev/null -w '%{http_code}\n'
```
Répéter 3-5 pour `api.pschneider.fr` et `blogs.pschneider.fr` (prod) et
les trois domaines preprod. Sans curl HTTP/3, un navigateur : DevTools →
Network → colonne « Protocole » → `h3` après mise à niveau.

## Comment ça marche (template + envsubst)

`contrib/template.vhost` contient les placeholders `$DOMAIN`,
`$BACKEND_SCHEME`, `$HTTP_HOST`, `$BACKEND_PORT`, `$QUIC_OPTS`. La règle
`generated/nginx/$(APP_INSTALL_PREFIX)yavsc%` du `Makefile` les substitue
via `envsubst` avec une **allowlist** :

```makefile
envsubst '$DOMAIN $BACKEND_SCHEME $HTTP_HOST $BACKEND_PORT $QUIC_OPTS'
```

Cette allowlist limite la substitution aux 5 variables citées, de sorte
que les variables nginx intrinsèques (`$host`, `$scheme`, `$remote_addr`,
`$proxy_add_x_forwarded_for`, `$request_uri`) survivent intactes. Sans
allowlist, `envsubst` détruirait ces variables nginx.

## Hors périmètre

- `docker-compose` / `Dockerfile` : non touchés (déploiement bare-metal
  systemd + nginx).
- Backend HTTP/2 vers Kestrel : nginx open-source ne supporte que
  HTTP/1.1 en upstream ; on garde `proxy_http_version 1.1`.
- `ssl_protocols TLSv1.3` (suivant `contrib/quic.md`) désactive TLS 1.2
  côté TCP aussi. Pour conserver la compat TLS 1.2 sur le fallback TCP,
  relâcher en `ssl_protocols TLSv1.2 TLSv1.3;` (QUIC reste en 1.3).