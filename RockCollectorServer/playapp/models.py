from django.db import models

class Group(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    name = models.CharField(max_length=100)

    def __str__(self):
        return self.name

class Score(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    user = models.ForeignKey('auth.User', on_delete=models.CASCADE)
    score_type = models.CharField(max_length=100)
    value = models.FloatField()
    groups = models.ManyToManyField(Group)