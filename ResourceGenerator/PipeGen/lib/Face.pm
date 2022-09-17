use strict;
use warnings;
package Face;
use base qw(Vertices);
use Math::Vector::Real;

sub UV { $_[0]->{uvs} ? $_[0]->{uvs}->[$_[1]] : undef }
sub UVs { wantarray ? @{$_[0]->{uvs}||[]} : scalar(@{$_[0]->{uvs}||[]}) }

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new(@_);
	# Other subconstructor stuff here
	return $self;
}

sub clone
{
	my $self = shift;
	my $clone = $self->SUPER::clone(@_);
	$clone->{uvs} = [
			map { $_->clone }
			@{$self->{uvs}}
		];
	return $clone;
}

sub FaceNormal
{
	my $self = shift;
	return $self->{normal}
		if exists $self->{normal};
	return $self->{normal} =
		$self->CalcFaceNormal;
}


sub RotateUVs
{
	my ($self, $angle) = @_;
	my $cos = cos($angle);
	my $sin = sin($angle);
	foreach my $uv ($self->UVs)
	{
		$uv->set(V(
			$cos * $uv->[0] - $sin * $uv->[1],
			$sin * $uv->[0] + $cos * $uv->[1],
		));
	}
	return $self;
}

sub ToWaveFrontObj
{
	my $self = shift;

	my @parts;

	for (my $i = 0; $i < $self->VertexCount; $i++)
	{
		my $uv = $self->UV($i);
		my $vert = $self->Vertice($i);
		if (defined $uv->[2])
		{
			push @parts, sprintf("%d/%d/%d",
				$vert->Position->[3] + 1,
				$uv->[2] + 1,
				$vert->Normal->[3] + 1
			);
		}
		else
		{
			push @parts, sprintf("%d//%d",
				$vert->Position->[3] + 1,
				$vert->Normal->[3] + 1
			);
		}
	}

	return "f " . join(" ", @parts);
}

1;