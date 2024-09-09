from rest_framework import serializers

import playapp.models as models


class ScoreSerializer(serializers.ModelSerializer):
    class Meta:
        model = models.Score
        fields = '__all__'