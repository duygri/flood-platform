# Research: lựa chọn và bằng chứng

**Ngày kiểm tra**: 2026-10-19. Chỉ dùng tài liệu upstream/chủ sở hữu sản phẩm.
Các quyết định dưới đây là thiết kế, chưa phải kết quả benchmark hoặc smoke test.

## R1. Spatial stack và kiểm tra biên

**Decision**: net10.0, EF Core10, Npgsql EF10 + plugin NetTopologySuite; PostGIS
geometry SRID4326. ST_Covers bao gồm biên, không ST_Contains.
**Rationale**: Npgsql có release10 và mapping NTS Covers sang SQL; PostGIS mô tả
Covers và spatial index. Kiểm engine thật, không dựa vào EF InMemory hoặc NTS tính mét.
**Alternatives**: NTS-only không chứng minh SQL; Contains loại điểm trên biên.
**Sources**: [Npgsql10](https://www.npgsql.org/efcore/release-notes/10.0.html),
[NTS mapping](https://www.npgsql.org/efcore/mapping/nts.html),
[ST_Covers](https://www.postgis.net/docs/ST_Covers.html).

## R2. Hai lựa chọn PostgreSQL có PostGIS

**Decision**: Chọn Azure Database for PostgreSQL Flexible Server; Supabase Postgres
là lựa chọn khả thi dự phòng, không dùng cả hai cùng lúc.
**Rationale**: Cùng cloud và có thể cùng region App Service; allowlist postgis ở
azure.extensions rồi CREATE EXTENSION bằng migration/provisioning principal.
**Alternatives**: Supabase có PostGIS; kết nối direct/session pooler tùy mạng,
không transaction pooler cho Hangfire có session locking. Direct có ràng buộc IPv6;
không dùng Supabase Auth, không expose bảng Identity/history qua Data API.
**Sources**: [Azure extension setup](https://learn.microsoft.com/en-us/azure/postgresql/extensions/how-to-create-extensions),
[extension versions](https://learn.microsoft.com/en-us/azure/postgresql/extensions/concepts-extensions-versions),
[Supabase PostGIS](https://supabase.com/docs/guides/database/extensions/postgis),
[Supabase connections](https://supabase.com/docs/guides/database/connecting-to-postgres).
Không suy ra giá/region/khả dụng thực tế từ tài liệu: kiểm subscription trước deploy.

## R3. Hangfire persisted cleanup

**Decision**: Hangfire Server trong API process; cùng PostgreSQL, schema riêng.
**Rationale**: Một nhu cầu background rõ ràng là xoá30 ngày, có retry và state bền.
**Alternatives**: Worker riêng tăng deployment; timer thuần không có persisted state.
**Limit**: Hangfire.PostgreSql là provider cộng đồng. Pin và kiểm tương thích với
.NET/Npgsql thực tế, startup/schema/migration, restart và lock trước release.
Basic+ Always On là ràng buộc App Service cho scheduler, không dùng Free idle host.
**Sources**: [provider source](https://github.com/hangfire-postgres/Hangfire.PostgreSql),
[App Service Always On](https://learn.microsoft.com/en-us/azure/app-service/configure-common),
[Hangfire always running](https://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html).

## R4. Identity không đồng nghĩa JWT

**Decision**: Identity user store/password verifier, custom login slice phát JWT
chuẩn15 phút + auth_sessions revocation; không expose registration endpoints.
**Rationale**: Tài liệu Identity nói bearer tích hợp là proprietary token, không
JWT. JWT RAM hợp yêu cầu hai domain, nhưng phải bảo vệ XSS và thu hồi server-side.
**Alternatives**: Cookie HttpOnly an toàn khỏi JS đọc token, nhưng cross-site dùng
SameSite=None;Secure + credentials/CSRF và có thể bị browser chặn. JWT localStorage
bị loại vì token tồn tại sau phiên trang. OIDC issuer chuẩn phù hợp production
nhiều client hơn, chưa thêm trong MVP demo first-party.
**Sources**: [Identity SPA](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0),
[JWT validation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0),
[CORS](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0).
Lựa chọn JWT tự phát giới hạn demo là đánh đổi của nhóm, không coi là khuyến nghị
thay OIDC cho mọi ứng dụng production.

## R5. MapLibre với OSM

**Decision**: MapLibre dùng raster source từ nhà cung cấp dữ liệu OSM cấu hình được.
**Rationale**: MapLibre hỗ trợ raster; attribution phải hiển thị. Public tile OSM
có usage policy, không SLA, không bulk download/prefetch offline.
**Alternatives**: Tile provider có hợp đồng khi cần bảo đảm, tự host chỉ sau khi
có bằng chứng về quota/SLA hoặc yêu cầu offline.
**Sources**: [MapLibre raster source](https://maplibre.org/maplibre-gl-js/docs/examples/add-a-raster-tile-source/),
[OSM tile policy](https://operations.osmfoundation.org/policies/tiles/).
Không che attribution, không dùng test tải hàng loạt tile công khai.

## R6. Secrets và deploy

**Decision**: Azure OIDC cho Actions, Vercel CLI deploy sau check; public config web
chỉ URL/API/tile/zone, không seed secret hoặc JWT private key.
**Rationale**: Secret frontend sẽ lộ nếu đưa vào bundle; NEXT_PUBLIC được bundle
lúc build. Tách staging origin và credentials production.
**Alternatives**: Publish profile dài hạn chỉ fallback vận hành sau khi đánh giá;
không đưa vào repo hoặc chạy deploy từ fork.
**Sources**: [Azure Actions](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions),
[Next env](https://nextjs.org/docs/app/guides/environment-variables),
[Vercel GitHub Actions](https://vercel.com/guides/how-can-i-use-github-actions-with-vercel).

## R7. Test và những điều chưa kiểm chứng

**Decision**: Testcontainers PostGIS thay SQLite/InMemory; Playwright deterministic
bằng geolocation/network stubs và tile fixture, sau đó điện thoại thực.
**Source**: [Testcontainers PostgreSQL](https://testcontainers.com/modules/postgresql/).
**Release gates**: pin image thực có hỗ trợ extension; build .NET10/Npgsql/provider;
kiểm fixture vùng bao phủ thực; quota cloud; smartphone GPS/WebGL và hiệu năng.
Đây là công việc thực thi kế hoạch, không phải câu hỏi yêu cầu còn bỏ ngỏ.
