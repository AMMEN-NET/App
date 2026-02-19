# Checklist: Implementar Tests en tu Proyecto

Sigue este checklist para implementar tests unitarios en todos tus componentes y servicios.

---

## 📋 FASE 1: Configuración Inicial

### Verificar dependencias instaladas
```bash
npm list @angular/core @angular/common
npm list jasmine karma karma-jasmine karma-chrome-launcher
npm list @angular/platform-browser-dynamic
```

Si algo falta:
```bash
npm install --save-dev jasmine karma karma-jasmine karma-chrome-launcher @angular/platform-browser-dynamic
```

### ✅ Verificar karma.conf.js existe
- [ ] Archivo `karma.conf.js` en la raíz del proyecto
- [ ] Contiene configuración de navegadores (Chrome, ChromeHeadless)

---

## 📋 FASE 2: Crear Tests para Componentes

### **TAREA 1: mis-favoritos Component**
- [x] Archivo [mis-favoritos.spec.ts](src/app/mis-favoritos/mis-favoritos.spec.ts) creado
- [x] Tests de Creación y Inicialización
- [x] Tests de Carga de Favoritos
- [x] Tests de Eliminación de Favoritos
- [x] Tests de Manejo de Señales
- [x] Tests de Integración HTML y lógica
- [ ] Ejecutar: `ng test --include='**/mis-favoritos.spec.ts'`
- [ ] Verificar que todos pasen ✅

### **TAREA 2: home Component**
- [ ] Crear archivo `src/app/home/home.component.spec.ts`
- [ ] Implementar tests básicos:
  - Creación del componente
  - Carga de datos iniciales
  - Interacción con servicios
  - Manejo de errores
- [ ] Ejecutar tests

### **TAREA 3: las-experiencias Component**
- [ ] Crear archivo `src/app/las-experiencias/experiencias.component.spec.ts`
- [ ] Tests para búsqueda de experiencias
- [ ] Tests para filtrado y ordenamiento
- [ ] Tests para navegación

### **TAREA 4: los-viajeros Component**
- [ ] Crear archivo `src/app/los-viajeros/viajeros.component.spec.ts`
- [ ] Tests para listar viajeros
- [ ] Tests para interacción con perfiles

### **TAREA 5: mi-perfil-personalizado Component**
- [ ] Crear archivo `src/app/mi-perfil-personalizado/mi-perfil-personalizado.component.spec.ts`
- [ ] Tests para mostrar datos del perfil
- [ ] Tests para editar perfil
- [ ] Tests para cambiar foto

### **TAREA 6: mis-calificaciones Component**
- [ ] Crear archivo `src/app/mis-calificaciones/mis-calificaciones.spec.ts`
- [ ] Tests para mostrar calificaciones
- [ ] Tests para agregar calificación
- [ ] Tests para editar/eliminar

### **TAREA 7: admin-dashboard Component**
- [ ] Crear archivo `src/app/admin/dashboard/admin-dashboard.component.spec.ts`
- [ ] Tests para mostrar métricas
- [ ] Tests para filtros
- [ ] Tests para descargar reportes

---

## 📋 FASE 3: Crear Tests para Servicios

### **TAREA 8: ListaDeFavoritosService**
- [x] Template preparado en `TESTING_SERVICES.md`
- [ ] Crear archivo `src/app/proxy/lista-de-favoritos/lista-de-favoritos.service.spec.ts`
- [ ] Tests para `obtenerFavoritos()`
- [ ] Tests para `agregarAFavoritos()`
- [ ] Tests para `eliminarDeFavoritos()`
- [ ] Tests para `esFavorito()`
- [ ] Tests para `contarFavoritos()`
- [ ] Ejecutar tests

### **TAREA 9: DashboardService**
- [x] Template preparado en `TESTING_SERVICES.md`
- [ ] Crear archivo `src/app/proxy/estadisticas/dashboard.service.spec.ts`
- [ ] Tests para obtener métricas
- [ ] Tests para ciudades más consultadas
- [ ] Tests para tiempos promedio
- [ ] Tests para logs del sistema

### **TAREA 10: ExperienciaService**
- [x] Template preparado en `TESTING_SERVICES.md`
- [ ] Crear archivo `src/app/proxy/experiencias/experiencia.service.spec.ts`
- [ ] Tests para búsqueda por ciudad
- [ ] Tests para obtener por ID
- [ ] Tests para crear/editar/eliminar
- [ ] Tests para búsqueda por nombre

### **TAREA 11: FotoPerfilService**
- [x] Template preparado en `TESTING_SERVICES.md`
- [ ] Crear archivo `src/app/proxy/controllers/foto-perfil.service.spec.ts`
- [ ] Tests para obtener foto
- [ ] Tests para subir foto (validación de tipo y tamaño)
- [ ] Tests para eliminar foto
- [ ] Tests para actualizar foto

### **TAREA 12: CuentaService**
- [ ] Crear archivo `src/app/proxy/cuentas/cuenta.service.spec.ts`
- [ ] Tests para obtener datos de cuenta
- [ ] Tests para actualizar cuenta
- [ ] Tests para cambiar contraseña

### **TAREA 13: DestinoService**
- [ ] Crear archivo `src/app/proxy/destinos/destino.service.spec.ts`
- [ ] Tests para búsqueda de destinos
- [ ] Tests para obtener detalles
- [ ] Tests para API externa (GeoDB)

---

## 📋 FASE 4: Tests de Integración

### **TAREA 14: Rutas**
- [ ] Crear archivo `src/app/app.routes.spec.ts`
- [ ] Tests para que todas las rutas estén configuradas
- [ ] Tests para navegación entre componentes
- [ ] Tests para protección de rutas (guards)

### **TAREA 15: Interceptores**
- [ ] Si tienes interceptores HTTP, crear tests
- [ ] Tests para agregar headers
- [ ] Tests para manejo de errores global

---

## 📋 FASE 5: Cobertura de Código

### **TAREA 16: Medir Cobertura**
```bash
ng test --no-watch --code-coverage
```

- [ ] Abrir reporte: `coverage/index.html`
- [ ] Revisar porcentaje de cobertura
- [ ] Meta: >80% en líneas (Statements)
- [ ] Meta: >75% en ramas (Branches)
- [ ] Meta: >80% en funciones (Functions)
- [ ] Meta: >80% en líneas ejecutadas (Lines)

### **TAREA 17: Mejorar Cobertura**
- [ ] Identificar archivos con baja cobertura
- [ ] Agregar tests para casos no cubiertos
- [ ] Especialmente para manejo de errores

---

## 📋 FASE 6: CI/CD

### **TAREA 18: Configurar Tests en CI/CD**
Si usas GitHub Actions, GitLab CI, o similar:

```bash
ng test --watch=false --code-coverage --browsers=ChromeHeadless
```

- [ ] Agregar script a `package.json`:
```json
{
  "scripts": {
    "test": "ng test --watch=false",
    "test:coverage": "ng test --no-watch --code-coverage"
  }
}
```

- [ ] Configurar pipeline en `.github/workflows/test.yml`
- [ ] Tests corren automáticamente en cada push

---

## 📊 Estructura de Archivos Esperada

Después de completar todas las tareas, tendrás:

```
src/app/
├── home/
│   ├── home.component.ts
│   ├── home.component.html
│   └── home.component.spec.ts         ✅
├── mis-favoritos/
│   ├── mis-favoritos.ts
│   ├── mis-favoritos.html
│   └── mis-favoritos.spec.ts          ✅
├── las-experiencias/
│   ├── experiencias.component.ts
│   └── experiencias.component.spec.ts  ✅
├── los-viajeros/
│   ├── viajeros.component.ts
│   └── viajeros.component.spec.ts     ✅
├── mi-perfil-personalizado/
│   └── mi-perfil-personalizado.component.spec.ts ✅
├── mis-calificaciones/
│   └── mis-calificaciones.spec.ts     ✅
├── admin/
│   └── dashboard/
│       ├── admin-dashboard.component.ts
│       └── admin-dashboard.component.spec.ts ✅
├── proxy/
│   ├── lista-de-favoritos/
│   │   └── lista-de-favoritos.service.spec.ts ✅
│   ├── estadisticas/
│   │   └── dashboard.service.spec.ts    ✅
│   ├── experiencias/
│   │   └── experiencia.service.spec.ts  ✅
│   ├── destinos/
│   │   └── destino.service.spec.ts      ✅
│   ├── cuentas/
│   │   └── cuenta.service.spec.ts       ✅
│   └── controllers/
│       └── foto-perfil.service.spec.ts  ✅
└── app.routes.spec.ts                   ✅
```

---

## ⏱️ Estimación de Tiempo

| Fase | Tareas | Horas |
|------|--------|-------|
| 1. Configuración | 1 | 0.5 |
| 2. Tests Componentes | 7 | 8-10 |
| 3. Tests Servicios | 6 | 6-8 |
| 4. Tests Integración | 2 | 2-3 |
| 5. Cobertura | 2 | 2-3 |
| 6. CI/CD | 1 | 1-2 |
| **TOTAL** | **19** | **~20-27 horas** |

*Nota: Esto es aproximado. Depende de la complejidad de tu código.*

---

## 🚀 Comandos Clave

### Desarrollo
```bash
# Ejecutar tests en modo watch
ng test

# Ejecutar tests una sola vez
ng test --watch=false

# Tests de un archivo específico
ng test --include='**/mis-favoritos.spec.ts'

# Tests de un patrón
ng test --include='**/*.service.spec.ts'
```

### Cobertura
```bash
# Generar reporte de cobertura
ng test --no-watch --code-coverage

# Abrir reporte en navegador
start coverage/index.html  # Windows
open coverage/index.html   # Mac
```

### CI/CD
```bash
# Modo headless (sin interfaz gráfica)
ng test --watch=false --browsers=ChromeHeadless

# Con cobertura en CI/CD
ng test --no-watch --code-coverage --browsers=ChromeHeadless
```

---

## 📚 Recursos

- **TESTING_GUIDE.md** - Guía completa de Jasmine y testing
- **TESTING_SERVICES.md** - Ejemplos de tests para servicios
- **TESTING_TEMPLATES.md** - Templates para crear tests rápido
- **mis-favoritos.spec.ts** - Ejemplo completo implementado

---

## ✅ Validación Final

Una vez completado todo:

```bash
# 1. Ejecutar todos los tests
ng test --watch=false

# 2. Verificar cobertura
ng test --no-watch --code-coverage

# 3. Revisar salida de coverage/index.html
# Meta: >80% de cobertura

# 4. Commit de todos los tests
git add "**/*.spec.ts"
git commit -m "test: agregar suite completa de unit tests"
```

---

## 🎯 Success Criteria

- ✅ Todos los tests pasan (`ng test`)
- ✅ Cobertura >80% en todas las categorías
- ✅ Cada componente/servicio tiene tests
- ✅ Tests siguen patrón AAA (Arrange-Act-Assert)
- ✅ Nombres descriptivos en `it()`
- ✅ Mocks apropiados para dependencias
- ✅ Pruebas de casos exitosos Y errores
- ✅ Documentación en README.md

---

## 🤝 Próximos Pasos

1. **Completar la lista**: Marca cada tarea conforme termines
2. **Ejecutar regularmente**: `npm run test` en cada cambio
3. **TDD (Test-Driven Development)**: Escribe tests ANTES de código
4. **Mantener cobertura**: Nuevas features deben venir con tests
5. **Code Review**: Verifica tests en PRs antes de merging

¡Éxito! 🚀
