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