# Proyecto Capilla — modelado 3D de la finca

| Carpeta | Contenido |
|---|---|
| `videos/` | Vídeos originales de la finca |
| `fotogramas/` | Fotogramas extraídos de los vídeos (una subcarpeta por vídeo) |
| `referencias/` | Planos, capturas de Catastro / Google Maps, fotos |
| `medidas/` | Medidas reales tomadas en campo (`medidas.md`) |
| `revit/` | Archivo del proyecto Revit (`Capilla.rvt`) |
| `revit/backups/` | Copias de seguridad del modelo |
| `renders/` | Imágenes y vistas exportadas |
| `scripts/` | Scripts auxiliares (extracción de fotogramas, envío a Revit) |

## Modelo y revisión actuales

El modelo actualizado es [Capilla_ajuste_nuevas_tomas.rvt](revit/Capilla_ajuste_nuevas_tomas.rvt).
La [revisión del 12 de octubre de 2026](analisis/nuevas_tomas_2026-10-12.html)
reúne las nuevas referencias, los cambios de ubicación y las vistas exportadas.
El [manifiesto](revit/Capilla_ajuste_nuevas_tomas.manifest.json) registra las
comprobaciones, los archivos de referencia y las limitaciones del modelo.

Se conservan las versiones anteriores y la copia de los movimientos realizados
por el propietario en `revit/backups/`. Las nuevas fotos y vídeos están en
`referencias_nuevas/2026-10-12/`. Las medidas y cotas siguen siendo estimadas;
los interiores completos y la vegetación detallada están pendientes.

## Descargar el proyecto completo

Los archivos de Revit y los vídeos se almacenan con Git LFS. Para descargar
sus contenidos completos, utiliza Git con Git LFS instalado:

```sh
git lfs install
git clone https://github.com/AleatorioAgencia/Capilla.git
cd Capilla
git lfs pull
```

El repositorio incluye referencias, fotogramas, renders, análisis, scripts y
copias de seguridad. Las cachés, dependencias instaladas y productos temporales
de compilación se excluyen mediante `.gitignore`.
