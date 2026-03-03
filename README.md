# Questions TP

### Comment garantissez-vous l'unicité du numéro de dossier ?

Au niveau base de données, via Fluent API dans PatientConfiguration :
```csharp
builder.HasIndex(p => p.RecordNumber)
.IsUnique()
.HasDatabaseName("IX_Patients_RecordNumber");
```

### Quelle stratégie utilisez-vous pour les clés primaires ?

Guid généré côté C#
```csharp
public Guid Id { get; init; } = Guid.NewGuid();
```
+ ValueGeneratedNever() dans la configuration pour empêcher Postgres de générer un Id différent de celui en mémoire.
```csharp
builder.Property(p => p.Id)
.ValueGeneratedNever();
```

### Comment validez-vous la date de naissance ?

A l'aide du type DateOnly pour le moment.

### Quelle relation entre Doctor et Department ?

C'est une relation ManyToOne, un médecin appartient à un seul département, un département contient plusieurs médecins.
On a aussi une relation OneToOne pour le directeur médical du département qui point vers un médecin.

### Quel DeleteBehavior ? Justifiez votre choix.

Doctor → Department : Restrict
Si je supprime un département qui contient encore des médecins, la suppression est bloquée. On doit réaffecter les médecins avant de pouvoir supprimer le département.
Department → Doctor : SetNull
Si je supprime un médecin qui est responsable d'un département, MedicalDirectorId passe à null. Le département peut ne pas avoir de responsable assigné, en attendant qu'on en assigne un nouveau.

### Comment paginer les résultats ?

On utilise .Skip((page - 1) * pageSize).Take(pageSize) dans la requête LINQ, combiné à un CountAsync() séparé pour connaître le nombre total d'éléments.

### Comment optimiser les requêtes fréquentes ?

On ajoute des index sur les colonnes filtrées fréquemment via HasIndex() dans la configuration  PostgreSQL utilise l'index au lieu de parcourir toute la table.

### Comment éviter de charger des données inutiles ?

On utilise .AsNoTracking() pour les lectures et .Select() pour ne projeter que les colonnes qu'on veut, plutôt que de charger toute l'entité.

### Quelle stratégie de chargement utiliser ?

La projection via .Select() est la meilleure stratégie pour les vues en lecture seule car elle laisse EF Core générer un SQL ciblé avec uniquement les colonnes nécessaires, sans charger toute une entité en mémoire.

### Comment éviter le problème N+1 ?

Ne jamais accéder à une navigation property dans une boucle sur des entités déjà chargées. Il faut tout faire dans un seul .Select() pour qu'EF Core génère un unique SQL avec les JOINs nécessaires.

### Comment structurer les données pour la vue ?

Créer un DTO dédié par vue qui reflète ce qu'affiche l'écran plutôt que de retourner des entités génériques.